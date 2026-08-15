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
            internal Func<string, bool, StoryCommands.StoryParseResult> ParseCommand;

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
            internal Func<bool?, UniTask> CharacterShow;
            internal Action<bool?> CharacterShowImmediate;
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

                var source = _ctx.GetNextText();
                var choices = _ctx.GetChoices();

                var parseResult = _ctx.ParseCommand(source, choices.Count > 0);
                if (!parseResult.IsSuccess)
                {
                    _ctx.OnLog((LogType.Error, $"[StoryParser] {parseResult.Error.Code}: {parseResult.Error.Message}\nSource: {parseResult.Error.Source}"));
                    continue;
                }

                var command = parseResult.Command;
                var value = command.Value;
                var args = command.Arguments;

                switch (command.Type)
                {
                    case StoryCommands.StoryCommandType.Empty:
                    case StoryCommands.StoryCommandType.Metadata:
                    case StoryCommands.StoryCommandType.Keyboard:
                        queue.Enqueue(new QueueProcess.EmptyQueue());
                        continue;

                    case StoryCommands.StoryCommandType.Notification:
                        queue.Enqueue(new QueueProcess.NotificationQueue
                        {
                            NotificationText = value,
                            ShowNotification = _ctx.ShowNotification
                        });
                        continue;

                    case StoryCommands.StoryCommandType.Location:
                        queue.Enqueue(new QueueProcess.BackgroundQueue.LocationQueue
                        {
                            SetImage = _ctx.SetImage,
                            SetImageImmediate = _ctx.SetImageImmediate,
                            AssetName = value,
                            Args = args
                        });
                        continue;

                    case StoryCommands.StoryCommandType.CutScene:
                        queue.Enqueue(new QueueProcess.BackgroundQueue.CutSceneQueue
                        {
                            SetImage = _ctx.SetImage,
                            SetImageImmediate = _ctx.SetImageImmediate,
                            AssetName = value,
                            Args = args
                        });
                        continue;

                    case StoryCommands.StoryCommandType.Music:
                    case StoryCommands.StoryCommandType.Sound:
                    case StoryCommands.StoryCommandType.Ambient:
                        var audioType = command.Type == StoryCommands.StoryCommandType.Music
                            ? Audio.Entity.Audio.Music
                            : command.Type == StoryCommands.StoryCommandType.Sound
                                ? Audio.Entity.Audio.Sound
                                : Audio.Entity.Audio.Ambient;
                        queue.Enqueue(new QueueProcess.AudioQueue
                        {
                            PlayAudio = assetName => _ctx.PlayAudio(assetName, audioType),
                            AssetName = value,
                        });
                        continue;

                    case StoryCommands.StoryCommandType.Camera:
                        queue.Enqueue(new QueueProcess.BackgroundQueue.CameraQueue
                        {
                            SetCamera = _ctx.SetCamera,
                            SetCameraImmediate = _ctx.SetCameraImmediate,
                            Value = value
                        });
                        continue;

                    case StoryCommands.StoryCommandType.Wait:
                        queue.Enqueue(new QueueProcess.AwaitQueue
                        {
                            Wait = _ctx.Wait,
                            Timer = command.WaitDuration
                        });
                        continue;

                    case StoryCommands.StoryCommandType.Dialogue:
                        queue = EnqueueDialogue(queue, command, choices, bubbleDone, ref lastCharacterName);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
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

        private Queue<QueueProcess.IQueue> EnqueueDialogue(
            Queue<QueueProcess.IQueue> queue,
            StoryCommands.StoryCommand command,
            List<Ink.Runtime.Choice> choices,
            UniTaskCompletionSource bubbleDone,
            ref string lastCharacterName)
        {
            var name = command.Name;
            var args = command.Arguments;
            queue = queue.EnqueueFirst(CreateSetBubbleQueue(command, choices, bubbleDone));

            if (string.IsNullOrEmpty(command.Name) && string.IsNullOrEmpty(command.Value))
            {
                return EnqueueBubbleLifecycle(queue, bubbleDone);
            }
            else
            {
                var dialogAlign = (name == _ctx.MainCharacter)
                ? TextAlignment.Left
                : (name == StoryContracts.StorySpeakers.Narrator || name == StoryContracts.StorySpeakers.Wardrobe)
                    ? TextAlignment.Center
                    : TextAlignment.Right;
                var characterName = name;
                if (args.Any(a => string.Equals(a, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase)))
                    characterName += "_child";
                var isNewCharacter = lastCharacterName != characterName;
                if (isNewCharacter)
                    lastCharacterName = characterName;

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
                return EnqueueBubbleLifecycle(queue, bubbleDone);
            }
        }

        private QueueProcess.BubbleQueue.SetBubbleQueue CreateSetBubbleQueue(
            StoryCommands.StoryCommand command,
            List<Ink.Runtime.Choice> choices,
            UniTaskCompletionSource bubbleDone)
        {
            return new QueueProcess.BubbleQueue.SetBubbleQueue
            {
                BubbleDone = bubbleDone,
                GetLocalizationValue = _ctx.GetLocalizationValue,
                Choices = choices,
                SetMainCharacterView = _ctx.SetMainCharacterView,
                SetMainCharacterClothes = _ctx.SetMainCharacterClothes,
                SetMainCharacterHair = _ctx.SetMainCharacterHair,
                SaveChoice = _ctx.SaveChoice,
                SetChoice = _ctx.SetChoice,
                Name = command.Name,
                Value = command.Value,
                Args = command.Arguments,

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
            };
        }

        private Queue<QueueProcess.IQueue> EnqueueBubbleLifecycle(
            Queue<QueueProcess.IQueue> queue,
            UniTaskCompletionSource bubbleDone)
        {
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
            return queue;
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
