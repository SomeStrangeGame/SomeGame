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

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            var queue = new Queue<QueueProcess.IQueue>();
            var lastCharacterName = string.Empty;

            //load here...

            await _ctx.HideLoading();

            while (!IsDisposed)
            {
                await UniTask.Yield();

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
                    queue.Enqueue(new QueueProcess.NotificationQueue
                    {
                        IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                        NotificationText = value,
                        ShowNotification = _ctx.ShowNotification
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    queue.Enqueue(new QueueProcess.BackgroundQueue.LocationQueue
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
                    queue.Enqueue(new QueueProcess.BackgroundQueue.CutSceneQueue
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
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Music),
                        AssetName = value
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("sound"))
                {
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Sound),
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("ambient"))
                {
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Ambient),
                        AssetName = value,
                    });
                    continue;
                }

                if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new QueueProcess.BackgroundQueue.CameraQueue
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
                        queue.Enqueue(new QueueProcess.AwaitQueue
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
                queue.Enqueue(new QueueProcess.CharacterQueue.DialogQueue
                {
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                    SetDialogue = _ctx.SetDialogue,
                    DialogAlign = dialogAlign
                });

                queue = queue.EnqueueFirst(new QueueProcess.LoadChoiceQueue
                {
                    BubbleDone = bubbleDone,
                    GetChoices = _ctx.GetChoices,
                    SetChoice = _ctx.SetChoice,
                    SetCharacterView = SetCharacterView,
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                    LoadChoice = _ctx.SaveSystem.LoadChoice,
                    Args = args,
                });

                queue = queue.EnqueueFirst(new QueueProcess.BubbleQueue.SetBubbleQueue
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

                var characterNameTemp = $"{name}";
                if (args.Any(a => a.ToLower() == "маленькая"))
                    characterNameTemp += "_child";
                var isNewCharacter = lastCharacterName != characterNameTemp;
                queue = queue.EnqueueFirst(new QueueProcess.CharacterQueue.HideCharacterQueue
                {
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                    CharacterHide = _ctx.CharacterHide,
                    CharacterHideImmediate = _ctx.CharacterHideImmediate,
                    IsNewCharacter = isNewCharacter,
                    OnHidecharacter = () => lastCharacterName = characterNameTemp,
                });


                queue.Enqueue(new QueueProcess.CharacterQueue.ShowCharacterQueue
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

                queue.Enqueue(new QueueProcess.BubbleQueue.ShowBubbleQueue
                {
                    BubbleDone = bubbleDone,
                    BubbleShow = _ctx.Bubble.Show,
                    BubbleShowImmediate = _ctx.Bubble.ShowImmediate,
                    IsLoadingInProcess = () => _ctx.SaveSystem.IsLoadingInProcess,
                });

                queue.Enqueue(new QueueProcess.BubbleQueue.HideBubbleQueue
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

    internal static class QueueExt
    {
        internal static Queue<T> EnqueueFirst<T>(this Queue<T> queue, T item)
        {
            var tempReversed = queue.Reverse().ToList();
            tempReversed.Add(item);
            tempReversed.Reverse();
            return new Queue<T>(tempReversed);
        }
    }
}

