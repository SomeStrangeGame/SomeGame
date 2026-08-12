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

            internal Func<string, bool, bool, string[], UniTask> SetImage;
            internal Func<string, bool, bool, string[], UniTask> SetImageImmediate;
            internal Func<string, UniTask> SetCamera;
            internal Func<string, UniTask> SetCameraImmediate;
            internal Func<TextAlignment, UniTask> SetDialogue;
            internal Func<TextAlignment, UniTask> SetDialogueImmediate;

            internal Func<float, UniTask> Wait;

            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;

            internal Func<string, string> GetLocalizationValue;

            internal Func<UniTask> BubbleShow;
            internal Action BubbleShowImmediate;
            internal Func<UniTask> BubbleHide;
            internal Action BubbleHideImmediate;
            internal Action<Bubble.Entity.BubbleScreenCtx> SetBubbleScreen;
            internal Action<Bubble.Entity.WardrobeScreenCtx> SetWardrobeScreen;
            internal Action<Bubble.Entity.ChooseScreenCtx> SetChooseScreen;

            internal List<byte> Save;
            internal Action<byte> SaveChoice;

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

            var save = new List<byte>(_ctx.Save);

            await _ctx.HideLoading();

            while (!IsDisposed)
            {
                await UniTask.Yield();

                var bubbleDone = new UniTaskCompletionSource();

                var text = _ctx.GetNextText();
                var choices = _ctx.GetChoices();

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();
                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();
                var dialogAlign = (name == _ctx.MainCharacter) 
                    ? TextAlignment.Left
                    : (name == "...")
                        ? TextAlignment.Center
                        : (name == "..." || name == "Wardrobe") 
                            ? TextAlignment.Center 
                            : TextAlignment.Right;
                var characterNameTemp = $"{name}";
                    if (args.Any(a => a.ToLower() == "маленькая"))
                        characterNameTemp += "_child";
                var isNewCharacter = lastCharacterName != characterNameTemp;
                if (isNewCharacter)
                    lastCharacterName = characterNameTemp;

                if (string.IsNullOrEmpty(text) && choices.Count == 0)
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "title")
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "series")
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "genres")
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "annotation")
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "stats")
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower().Contains("keyboard"))
                {
                    queue.Enqueue(new QueueProcess.EmptyQueue());
                    continue;
                }
                else if (prefix.ToLower() == "notification")
                {
                    queue.Enqueue(new QueueProcess.NotificationQueue
                    {
                        NotificationText = value,
                        ShowNotification = _ctx.ShowNotification
                    });
                    continue;
                }
                else if (prefix.ToLower().Contains("location"))
                {
                    queue.Enqueue(new QueueProcess.BackgroundQueue.LocationQueue
                    {
                        SetImage = _ctx.SetImage,
                        SetImageImmediate = _ctx.SetImageImmediate,
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                else if (prefix.ToLower().Contains("cut-scene"))
                {
                    queue.Enqueue(new QueueProcess.BackgroundQueue.CutSceneQueue
                    {
                        SetImage = _ctx.SetImage,
                        SetImageImmediate = _ctx.SetImageImmediate,
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                else if (prefix.ToLower().Contains("music"))
                {
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Music),
                        AssetName = value
                    });
                    continue;
                }
                else if (prefix.ToLower().Contains("sound"))
                {
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Sound),
                        AssetName = value,
                    });
                    continue;
                }
                else if (prefix.ToLower().Contains("ambient"))
                {
                    queue.Enqueue(new QueueProcess.AudioQueue
                    {
                        PlayAudio = assetName => _ctx.PlayAudio(assetName, Audio.Entity.Audio.Ambient),
                        AssetName = value,
                    });
                    continue;
                }
                else if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new QueueProcess.BackgroundQueue.CameraQueue
                    {
                        SetCamera = _ctx.SetCamera,
                        SetCameraImmediate = _ctx.SetCameraImmediate,
                        Value = value
                    });
                    continue;
                }
                else if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        queue.Enqueue(new QueueProcess.AwaitQueue
                        {
                            Wait = _ctx.Wait,
                            Timer = seconds
                        });
                    continue;
                }
                else
                {
                    queue = queue.EnqueueFirst(new QueueProcess.BubbleQueue.SetBubbleQueue
                    {
                        BubbleDone = bubbleDone,
                        GetLocalizationValue = _ctx.GetLocalizationValue,
                        Choices = choices,
                        SetMainCharacterView = _ctx.SetMainCharacterView,
                        SetMainCharacterClothes = _ctx.SetMainCharacterClothes,
                        SetMainCharacterHair = _ctx.SetMainCharacterHair,
                        SaveChoice = _ctx.SaveChoice,
                        SetChoice = _ctx.SetChoice,
                        Name = name,
                        Value = value,
                        Args = args,

                        SetBubbleScreen = data =>
                        {
                            _ctx.SetBubbleScreen(new Bubble.Entity.BubbleScreenCtx
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
                            _ctx.SetWardrobeScreen(new Bubble.Entity.WardrobeScreenCtx
                            {
                                //migrate wardrobe here...
                            });
                        },
                        SetChooseScreen = data =>
                        {
                            _ctx.SetChooseScreen(new Bubble.Entity.ChooseScreenCtx
                            {
                                // migrate choose here...
                            });
                        },
                    });
                    queue = queue.EnqueueFirst(new QueueProcess.CharacterQueue.HideCharacterQueue
                    {
                        CharacterHide = _ctx.CharacterHide,
                        CharacterHideImmediate = _ctx.CharacterHideImmediate,
                        IsNewCharacter = isNewCharacter,
                    });
                    queue.Enqueue(new QueueProcess.CharacterQueue.DialogQueue
                    {
                        SetDialogue = _ctx.SetDialogue,
                        SetDialogueImmediate = _ctx.SetDialogueImmediate,
                        DialogAlign = dialogAlign
                    });
                    queue.Enqueue(new QueueProcess.CharacterQueue.ShowCharacterQueue
                    {
                        CharacterSetImage = _ctx.CharacterSetImage,
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
                        BubbleShow = _ctx.BubbleShow,
                        BubbleShowImmediate = _ctx.BubbleShowImmediate,
                    });
                    queue.Enqueue(new QueueProcess.BubbleQueue.HideBubbleQueue
                    {
                        BubbleHide = _ctx.BubbleHide,
                        BubbleHideImmediate = _ctx.BubbleHideImmediate,
                    });
                }

                if (save.Count > 0)
                {
                    var result = save.First();
                    save.RemoveAt(0);
                    while (queue.TryDequeue(out var element))
                    {
                        await element.RunImmediate(result);
                    }
                }
                else
                {
                    while (queue.TryDequeue(out var element))
                    {
                        await element.Run();
                    }
                }
                
            }
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

