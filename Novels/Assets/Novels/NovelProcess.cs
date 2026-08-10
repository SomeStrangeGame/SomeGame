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
            internal Func<bool> IsLoadingInProcess;
            internal string NotificationText;
            internal Func<string, UniTask> ShowNotification;

            public async readonly UniTask Run()
            {
                if (!IsLoadingInProcess())
                        ShowNotification(NotificationText).Forget();
            }
        }
        private struct MusicQueue : IQueue
        {
            internal Func<string, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName);
            }
        }
        private struct SoundQueue : IQueue
        {
            internal Func<string, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName);
            }
        }
        private struct AmbientQueue : IQueue
        {
            internal Func<string, UniTask> PlayAudio;
            internal string AssetName;

            public async readonly UniTask Run()
            {
                await PlayAudio(AssetName);
            }
        }
        private struct LocationQueue : IQueue
        {
            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Func<bool> IsLoadingInProcess;
            internal string AssetName;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(IsLoadingInProcess(), AssetName, false, false, Args);
            }
        }
        private struct CutSceneQueue : IQueue
        {
            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Func<bool> IsLoadingInProcess;
            internal string AssetName;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                await SetImage(IsLoadingInProcess(), AssetName, true, false, Args);
            }
        }
        private struct CameraQueue : IQueue
        {
            internal Func<bool> IsLoadingInProcess;
            internal Func<bool, string, UniTask> SetCamera;
            internal string Value;

            public async readonly UniTask Run()
            {
                await SetCamera(IsLoadingInProcess(), Value);
            }
        }
        private struct AwaitQueue : IQueue
        {
            internal Func<bool> IsLoadingInProcess;
            internal Func<float, UniTask> Wait;
            internal float Timer;

            public async readonly UniTask Run()
            {
                if (!IsLoadingInProcess())
                    await Wait(Timer);
            }
        }
        private struct DialogQueue : IQueue
        {
            internal Func<bool, TextAlignment, UniTask> SetDialogue;
            internal Func<bool> IsLoadingInProcess;
            internal TextAlignment DialogAlign;

            public async readonly UniTask Run()
            {
                await SetDialogue(IsLoadingInProcess(), DialogAlign);
            }
        }
        private struct HideCharacterQueue : IQueue
        {
            internal Func<bool> IsLoadingInProcess;
            internal Func<UniTask> CharacterHide;
            internal Action CharacterHideImmediate;
            internal bool IsNewCharacter;
            internal Action OnHidecharacter;

            public async readonly UniTask Run()
            {
                if (IsNewCharacter)
                {
                    OnHidecharacter();
                    if (!IsLoadingInProcess())
                        await CharacterHide();
                    else
                        CharacterHideImmediate();
                }
            }
        }
        private struct ShowCharacterQueue : IQueue
        {
            internal Func<string, string[], UniTask> CharacterSetImage;
            internal Func<bool> IsLoadingInProcess;
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
                    if (!IsLoadingInProcess())
                        await CharacterShow(Name == MainCharacter);
                    else
                        CharacterShowImmediate(Name == MainCharacter);
                }
            }
        }
        internal class SetBubbleQueue : IQueue
        {
            internal struct BubbleCtx
            {
                internal struct TextCtx
                {
                    internal string Header;
                    internal string Text;
                }

                internal struct ButtonCtx
                {
                    internal int Id;
                    internal string Text;
                    internal Action<int> OnClick;
                }

                internal string Name;
                internal string[] Args;
                internal TextCtx Text;
                internal ButtonCtx[] Buttons;
                internal Action OnBackgroundClick;
            }

            internal struct WardrobeCtx
            {
                
            }

            internal struct ChooseCtx
            {
                
            }

            internal UniTaskCompletionSource BubbleDone;
            internal Func<string, string> GetLocalizationValue;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<string[], Ink.Runtime.Choice> SetCharacterView;
            internal Action<byte> SaveChoice;
            internal Func<bool> IsLoadingInProcess;
            internal Action<int> SetChoice;
            internal string Name;
            internal string Value;
            internal string[] Args;

            internal Action<BubbleCtx> SetBubbleScreen;
            internal Action<WardrobeCtx> SetWardrobeScreen;
            internal Action<ChooseCtx> SetChooseScreen;

            public async UniTask Run()
            {
                if (Name == "some wardrobe trigger")
                {
                    SetWardrobeScreen(new WardrobeCtx
                    {
                        // set wardrobe here...
                    });
                }
                else if (Name == "some choose trigger")
                {
                    SetChooseScreen(new ChooseCtx
                    {
                        //set choose here...
                    });
                }
                else
                {
                    SetBubbleScreen(new BubbleCtx
                    {
                        Name = Name,
                        Args = Args,
                        Text = new BubbleCtx.TextCtx
                        {
                            Header = GetLocalizationValue(Name),
                            Text = Value
                        },
                        Buttons = GetChoices().Select(c => new BubbleCtx.ButtonCtx
                        {
                            Id = c.index,
                            Text = c.text,
                            OnClick = id =>
                            {
                                SetCharacterView(Args, c);
                                if (!IsLoadingInProcess())
                                    SaveChoice((byte)id);
                                SetChoice(id);
                                BubbleDone.TrySetResult();
                            }
                        }).ToArray(),
                        OnBackgroundClick = () =>
                        {
                            if (!IsLoadingInProcess())
                                SaveChoice(255);
                            BubbleDone.TrySetResult();
                        }
                    });
                }
            }
        }
        private struct LoadChoiceQueue : IQueue
        {
            internal UniTaskCompletionSource BubbleDone;
            internal Func<bool> IsLoadingInProcess;
            internal Func<byte> LoadChoice;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<int> SetChoice;
            internal Action<string[], Ink.Runtime.Choice> SetCharacterView;
            internal string[] Args;

            public async readonly UniTask Run()
            {
                if (IsLoadingInProcess())
                {
                    var savedChoice = LoadChoice();
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
            internal Func<bool> IsLoadingInProcess;
            internal Func<UniTask> BubbleShow;
            internal Action BubbleShowImmediate;

            public async readonly UniTask Run()
            {
                if (!IsLoadingInProcess())
                    await BubbleShow();
                else
                    BubbleShowImmediate();

                await BubbleDone.Task;
            }
        }
        private struct HideBubbleQueue : IQueue
        {
            internal Func<bool> IsLoadingInProcess;
            internal Func<UniTask> BubbleHide;
            internal Action BubbleHideImmediate;

            public async readonly UniTask Run()
            {
                if (!IsLoadingInProcess())
                    await BubbleHide();
                else
                    BubbleHideImmediate();
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
                        IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                        NotificationText = value,
                        ShowNotification = _ctx.ShowNotification
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    queue.Enqueue(new LocationQueue
                    {
                        IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                        SetImage = _ctx.SetImage,
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("cut-scene"))
                {
                    queue.Enqueue(new CutSceneQueue
                    {
                        IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                        SetImage = _ctx.SetImage,
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("music"))
                {
                    queue.Enqueue(new MusicQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Music),
                        AssetName = value
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("sound"))
                {
                    queue.Enqueue(new SoundQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Sound),
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("ambient"))
                {
                    queue.Enqueue(new AmbientQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Ambient),
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new CameraQueue
                    {
                        IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
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
                            IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                            Wait = _ctx.Wait,
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
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
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
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                    LoadChoice = _ctx.SaveSystem.LoadChoice,
                    Args = args,
                });
                tempQueueLoadChoice.Reverse();
                queue = new Queue<IQueue>(tempQueueLoadChoice);

                var tempQueueSetBubble = queue.Reverse().ToList();
                tempQueueSetBubble.Add(new SetBubbleQueue
                {
                    BubbleDone = bubbleDone,
                    GetLocalizationValue = _ctx.GetLocalizationValue,
                    GetChoices = _ctx.GetChoices,
                    SetCharacterView = SetCharacterView,
                    SaveChoice = _ctx.SaveSystem.SaveChoice,
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                    SetChoice = _ctx.SetChoice,
                    Name = name,
                    Value = value,
                    Args = args,

                    SetBubbleScreen = data =>
                    {
                        _ctx.Bubble.SetBubbleScreen(new Bubble.Entity.BubbleScreenCtx
                        {
                            Name = data.Name,
                            Args = data.Args,
                            Text = new Bubble.Entity.BubbleScreenCtx.TextCtx
                            {
                                Header = data.Text.Header,
                                Text = data.Text.Text,
                            },
                            Buttons = data.Buttons.Select(b => new Bubble.Entity.BubbleScreenCtx.ButtonCtx
                            {
                                Id = b.Id,
                                Text = b.Text,
                                OnClick = b.OnClick
                            }).ToArray(),
                            OnBackgroundClick = data.OnBackgroundClick
                        });
                    },
                    SetWardrobeScreen = data =>
                    {
                        _ctx.Bubble.SetWardrobeScreen(new Bubble.Entity.WardrobeScreenCtx
                        {
                            //migrate wardrobe here...
                        });
                    },
                    SetChooseScreen = data =>
                    {
                        _ctx.Bubble.SetChooseScreen(new Bubble.Entity.ChooseScreenCtx
                        {
                            // migrate choose here...
                        });
                    },
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
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
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
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
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
                    BubbleShow = _ctx.Bubble.Show,
                    BubbleShowImmediate = _ctx.Bubble.ShowImmediate,
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                });

                queue.Enqueue(new HideBubbleQueue
                {
                    BubbleHide = _ctx.Bubble.Hide,
                    BubbleHideImmediate = _ctx.Bubble.HideImmediate,
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
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

