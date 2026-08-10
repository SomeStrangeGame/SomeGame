using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal class NovelProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal string MainCharacter;

            internal Func<string> GetNextText;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<int> SetChoice;

            internal Func<string, UniTask> ShowNotification;

            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Func<bool, string, UniTask> SetCamera;
            internal Func<bool, TextAlignment, UniTask> SetDialogue;

            internal Func<float, UniTask> Wait;

            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;

            internal Func<string, string> GetLocalizationValue;

            internal Bubble.Entity Bubble;

            internal Save.Entity SaveSystem;

            internal Action<string> SetMainCharacterView;
            internal Action<string> SetMainCharacterClothes;
            internal Action<string> SetMainCharacterHair;
            internal Func<UniTask> CharacterHide;
            internal Action CharacterHideImmediate;
            internal Func<bool, UniTask> CharacterShow;
            internal Action<bool> CharacterShowImmediate;
            internal Func<string, string[], UniTask> CharacterSetImage;


            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;

            public Action<(LogType type, string message)> OnLog;
        }

        private interface IQueue
        {
            public UniTask Run();
        }
        
        private struct NotificationQueue : IQueue
        {
            internal Save.Entity SaveSystem;
            internal string NotificationText;
            internal Func<string, UniTask> ShowNotification;

            public async readonly UniTask Run()
            {
                if (!SaveSystem.IsLoadingInProcess)
                        ShowNotification(NotificationText).Forget();
            }
        }
        private struct MusicQueue : IQueue
        {
            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName, Audio.Entity.Audio.Music);
            }
        }
        private struct SoundQueue : IQueue
        {
            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName, Audio.Entity.Audio.Sound);
            }
        }
        private struct AmbientQueue : IQueue
        {
            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName, Audio.Entity.Audio.Ambient);
            }
        }
        private struct LocationQueue : IQueue
        {
            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Save.Entity SaveSystem;
            internal string AssetName;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(SaveSystem.IsLoadingInProcess, AssetName, false, false, Args);
            }
        }
        private struct CutSceneQueue : IQueue
        {
            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Save.Entity SaveSystem;
            internal string AssetName;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(SaveSystem.IsLoadingInProcess, AssetName, true, false, Args);
            }
        }
        private struct CameraQueue : IQueue
        {
            internal Save.Entity SaveSystem;
            internal Func<bool, string, UniTask> SetCamera;
            internal string Value;

            public async readonly UniTask Run()
            {
                await SetCamera(SaveSystem.IsLoadingInProcess, Value);
            }
        }
        private struct AwaitQueue : IQueue
        {
            internal Save.Entity SaveSystem;
            internal Func<float, UniTask> Wait;
            internal float Timer;

            public async readonly UniTask Run()
            {
                if (!SaveSystem.IsLoadingInProcess)
                    await Wait(Timer);
            }
        }
        private struct DialogQueue : IQueue
        {
            internal Func<bool, TextAlignment, UniTask> SetDialogue;
            internal Save.Entity SaveSystem;
            internal TextAlignment DialogAlign;

            public async readonly UniTask Run()
            {
                await SetDialogue(SaveSystem.IsLoadingInProcess, DialogAlign);
            }
        }
        private struct HideCharacterQueue : IQueue
        {
            internal Save.Entity SaveSystem;
            internal Func<UniTask> CharacterHide;
            internal Action CharacterHideImmediate;
            internal bool IsNewCharacter;
            internal Action OnHidecharacter;

            public async readonly UniTask Run()
            {
                if (IsNewCharacter)
                {
                    OnHidecharacter();
                    if (!SaveSystem.IsLoadingInProcess)
                        await CharacterHide();
                    else
                        CharacterHideImmediate();
                }
            }
        }
        private struct ShowCharacterQueue : IQueue
        {
            internal Func<string, string[], UniTask> CharacterSetImage;
            internal Save.Entity SaveSystem;
            internal Func<bool, UniTask> CharacterShow;
            internal Action<bool> CharacterShowImmediate;
            internal bool IsNewCharacter;
            internal string Name;
            internal string[] Args;
            internal string MainCharacter;

            public async readonly UniTask Run()
            {
                await CharacterSetImage(Name, Args);
                if (IsNewCharacter)
                {
                    if (!SaveSystem.IsLoadingInProcess)
                        await CharacterShow(Name == MainCharacter);
                    else
                        CharacterShowImmediate(Name == MainCharacter);
                }
            }
        }
        private class SetBubbleQueue : IQueue
        {
            internal UniTaskCompletionSource BubbleDone;
            internal Bubble.Entity Bubble;
            internal Func<string, string> GetLocalizationValue;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<string[], Ink.Runtime.Choice> SetCharacterView;
            internal Save.Entity SaveSystem;
            internal Action<int> SetChoice;
            internal string Name;
            internal string Value;
            internal string[] Args;

            public async UniTask Run()
            {
                if (Name == "some wardrobe trigger")
                {
                    // set wardrobe screen here...
                }
                else if (Name == "some choose trigger")
                {
                    //set choose screen here...
                }
                else
                {
                    Bubble.SetBubbleScreen(new Bubble.Entity.BubbleScreenCtx
                    {
                        Name = Name,
                        Args = Args,
                        Text = new Bubble.Entity.BubbleScreenCtx.TextCtx
                        {
                            Header = GetLocalizationValue(Name),
                            Text = Value
                        },
                        Buttons = GetChoices().Select(c => new Bubble.Entity.BubbleScreenCtx.ButtonCtx
                        {
                            Id = c.index,
                            Text = c.text,
                            OnClick = id =>
                            {
                                SetCharacterView(Args, c);
                                SaveSystem.TrySaveChoice((byte)id);
                                SetChoice(id);
                                BubbleDone.TrySetResult();
                            }
                        }).ToArray(),
                        OnBackgroundClick = () =>
                        {
                            SaveSystem.TrySaveChoice();
                            BubbleDone.TrySetResult();
                        }
                    });
                }
            }
        }
        private struct LoadChoiceQueue : IQueue
        {
            internal UniTaskCompletionSource BubbleDone;
            internal Save.Entity SaveSystem;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<int> SetChoice;
            internal Action<string[], Ink.Runtime.Choice> SetCharacterView;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                if (SaveSystem.TryLoadChoice(out var savedChoice))
                {
                    if (savedChoice != 255)
                    {
                        SetCharacterView(Args, GetChoices()[savedChoice]);
                        SetChoice(savedChoice);
                    }
                    BubbleDone.TrySetResult();
                }
            }
        }
        private struct ShowBubbleQueue : IQueue
        {
            internal UniTaskCompletionSource BubbleDone;
            internal Save.Entity SaveSystem;
            internal Bubble.Entity Bubble;

            public async readonly UniTask Run()
            {
                if (!SaveSystem.IsLoadingInProcess)
                    await Bubble.Show();
                else
                    Bubble.ShowImmediate();

                await BubbleDone.Task;
            }
        }

        private struct HideBubbleQueue : IQueue
        {
            internal Save.Entity SaveSystem;
            internal Bubble.Entity Bubble;

            public async readonly UniTask Run()
            {
                if (!SaveSystem.IsLoadingInProcess)
                    await Bubble.Hide();
                else
                    Bubble.HideImmediate();
            }
        }

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            var queue = new Queue<IQueue>();
            var lastCharacterName = string.Empty;

            await _ctx.HideLoading();

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                var text = _ctx.GetNextText();
                if (string.IsNullOrEmpty(text) && _ctx.GetChoices().Count == 0) continue;

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();
                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                if (prefix.ToLower() == "title") continue;
                if (prefix.ToLower() == "series") continue;
                if (prefix.ToLower() == "genres") continue;
                if (prefix.ToLower() == "annotation") continue;
                if (prefix.ToLower() == "stats") continue;
                if (prefix.ToLower().Contains("keyboard")) continue;

                if (prefix.ToLower() == "notification")
                {
                    queue.Enqueue(new NotificationQueue
                    {
                        SaveSystem = _ctx.SaveSystem,
                        NotificationText = value,
                        ShowNotification = _ctx.ShowNotification
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    queue.Enqueue(new LocationQueue
                    {
                        SaveSystem = _ctx.SaveSystem,
                        SetImage = _ctx.SetImage,
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("cut-scene"))
                {
                    queue.Enqueue(new CutSceneQueue{
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("music"))
                {
                    queue.Enqueue(new MusicQueue
                    {
                        PlayAudio = _ctx.PlayAudio,
                        AssetName = value
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("sound"))
                {
                    queue.Enqueue(new SoundQueue
                    {
                        PlayAudio = _ctx.PlayAudio,
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("ambient"))
                {
                    queue.Enqueue(new AmbientQueue
                    {
                        PlayAudio = _ctx.PlayAudio,
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new CameraQueue
                    {
                        SaveSystem = _ctx.SaveSystem,
                        SetCamera = _ctx.SetCamera,
                        Value = value
                    });
                    continue;
                }

                if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        queue.Enqueue(new AwaitQueue
                        {
                            Timer = seconds
                        });
                    continue;
                }

                TextAlignment dialogAlign;
                if (name == _ctx.MainCharacter)
                    dialogAlign = TextAlignment.Left;
                else if (name == "..." || name == "Wardrobe")
                    dialogAlign = TextAlignment.Center;
                else
                    dialogAlign = TextAlignment.Right;
                queue.Enqueue(new DialogQueue
                {
                    SaveSystem = _ctx.SaveSystem,
                    SetDialogue = _ctx.SetDialogue,
                    DialogAlign = dialogAlign
                });

                var tempQueueLoadChoice = queue.Reverse().ToList();
                tempQueueLoadChoice.Add(new LoadChoiceQueue
                {
                    BubbleDone = bubbleDone,
                    GetChoices = _ctx.GetChoices,
                    SetChoice = _ctx.SetChoice,
                    SetCharacterView = SetCharacterView,
                    SaveSystem = _ctx.SaveSystem,
                    Args = args,
                });
                tempQueueLoadChoice.Reverse();
                queue = new Queue<IQueue>(tempQueueLoadChoice);

                var tempQueueSetBubble = queue.Reverse().ToList();
                tempQueueSetBubble.Add(new SetBubbleQueue
                {
                    BubbleDone = bubbleDone,
                    Bubble = _ctx.Bubble,
                    GetLocalizationValue = _ctx.GetLocalizationValue,
                    GetChoices = _ctx.GetChoices,
                    SetCharacterView = SetCharacterView,
                    SaveSystem = _ctx.SaveSystem,
                    SetChoice = _ctx.SetChoice,
                    Name = name,
                    Value = value,
                    Args = args,
                });
                tempQueueSetBubble.Reverse();
                queue = new Queue<IQueue>(tempQueueSetBubble);

                var characterNameTemp = $"{name}";
                if (args.Any(a => a.ToLower() == "маленькая"))
                    characterNameTemp += "_child";
                var isNewCharacter = lastCharacterName != characterNameTemp;
                var tempQueueHideCharacter = queue.Reverse().ToList();
                tempQueueHideCharacter.Add(new HideCharacterQueue
                {
                    SaveSystem = _ctx.SaveSystem,
                    CharacterHide = _ctx.CharacterHide,
                    CharacterHideImmediate = _ctx.CharacterHideImmediate,
                    IsNewCharacter = isNewCharacter,
                    OnHidecharacter = () => lastCharacterName = characterNameTemp,
                });
                tempQueueHideCharacter.Reverse();
                queue = new Queue<IQueue>(tempQueueHideCharacter);

                queue.Enqueue(new ShowCharacterQueue
                {
                    CharacterSetImage = _ctx.CharacterSetImage,
                    SaveSystem = _ctx.SaveSystem,
                    CharacterShow = _ctx.CharacterShow,
                    CharacterShowImmediate = _ctx.CharacterShowImmediate,
                    Name = name,
                    IsNewCharacter = isNewCharacter,
                    Args = args,
                    MainCharacter = _ctx.MainCharacter,
                });

                queue.Enqueue(new ShowBubbleQueue
                {
                    BubbleDone = bubbleDone,
                    Bubble = _ctx.Bubble,
                    SaveSystem = _ctx.SaveSystem,
                });

                queue.Enqueue(new HideBubbleQueue
                {
                    Bubble = _ctx.Bubble,
                    SaveSystem = _ctx.SaveSystem,
                });

                while(queue.TryDequeue(out var element))
                {
                    await element.Run();
                }

                queue.Clear();
            }
        }

        private void SetCharacterView(string[] args, Ink.Runtime.Choice choice)
        {
            if (args.Any(a => a == "Выбери внешность"))
                _ctx.SetMainCharacterView(choice.text);
            if (args.Any(a => a == "Выбери одежду"))
                _ctx.SetMainCharacterClothes(choice.text);
            if (args.Any(a => a == "Выбери прическу" || a == "Выбери причёску"))
                _ctx.SetMainCharacterHair(choice.text);
        }
    }
}

