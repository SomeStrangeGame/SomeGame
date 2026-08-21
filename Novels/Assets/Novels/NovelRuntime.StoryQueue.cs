using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private static StoryProcessor.Entity CreateStoryProcessor(
            IBaseDisposable owner,
            string storyText,
            string initialState,
            string sourceMapText)
        {
            return new StoryProcessor.Entity(new StoryProcessor.Entity.Ctx
            {
                StoryText = storyText,
                InitialState = initialState,
                SourceMapText = sourceMapText,
            }).AddTo(owner);
        }

        private StoryQueue.StoryQueueBuilder CreateStoryQueue(
            StoryProcessor.Entity storyProcessor,
            Notification.NotificationController notification,
            Location.LocationController location,
            CancellationToken cancellationToken,
            Audio.AudioController audio,
            Bubble.BubbleController bubble,
            Wardrobe.WardrobeController wardrobe,
            Choose.ChooseController choose,
            System.Func<string, Cysharp.Threading.Tasks.UniTask<UnityEngine.Sprite>> loadChooseThumbnail,
            Save.SaveSystem save,
            Character.CharacterController character)
        {
            return new StoryQueue.StoryQueueBuilder(new StoryQueue.StoryQueueBuilder.Dependencies
            {
                Command = new StoryQueue.StoryQueueBuilder.CommandCtx
                {
                    ShowNotification = notification.Enqueue,
                    Location = new StoryQueue.StoryQueueBuilder.LocationCommandPort
                    {
                        SetImage = location.SetImage,
                        SetCamera = location.SetCamera,
                        Wait = seconds => Wait(seconds, cancellationToken),
                    },
                    Audio = new StoryQueue.StoryQueueBuilder.AudioPort
                    {
                        PlayMusic = assetName => audio.PlayAudio(assetName, Audio.AudioController.Audio.Music),
                        PlaySound = assetName => audio.PlayAudio(assetName, Audio.AudioController.Audio.Sound),
                        PlayAmbient = assetName => audio.PlayAudio(assetName, Audio.AudioController.Audio.Ambient),
                    },
                },

                Dialogue = new StoryQueue.StoryQueueBuilder.DialogueCtx
                {
                    MainCharacter = _definition.MainCharacter,

                    Location = new StoryQueue.StoryQueueBuilder.LocationDialoguePort
                    {
                        SetDialogue = location.SetDialogue,
                    },
                    Bubble = new StoryQueue.StoryQueueBuilder.BubblePort
                    {
                        Show = bubble.Show,
                        Hide = bubble.Hide,
                        SetBubbleScreen = bubble.SetBubbleScreen,
                    },
                    Wardrobe = new StoryQueue.StoryQueueBuilder.WardrobePort
                    {
                        Show = wardrobe.Show,
                        Hide = wardrobe.Hide,
                        SetScreen = wardrobe.SetScreen,
                    },
                    Choose = new StoryQueue.StoryQueueBuilder.ChoosePort
                    {
                        Show = choose.Show,
                        Hide = choose.Hide,
                        LoadThumbnail = loadChooseThumbnail,
                        SetScreen = choose.SetScreen,
                    },
                    Choice = new StoryQueue.StoryQueueBuilder.ChoicePort
                    {
                        SaveDecision = save.SaveDecision,
                        SetChoice = storyProcessor.SetChoice,
                    },
                    Character = new StoryQueue.StoryQueueBuilder.CharacterPort
                    {
                        SetMainCharacterView = character.SetMainCharacterView,
                        SetMainCharacterClothes = character.SetMainCharacterClothes,
                        SetMainCharacterHair = character.SetMainCharacterHair,
                        SetMainCharacterAccessory = character.SetMainCharacterAccessory,
                        LoadWardrobeThumbnail = character.LoadWardrobeThumbnail,
                        PreviewWardrobeChoice = character.PreviewWardrobeChoice,
                        CharacterHide = character.Hide,
                        CharacterShow = character.Show,
                        CharacterSetImage = character.SetImage,
                    },
                },
            });
        }

        private static async UniTask Wait(
            float seconds,
            CancellationToken cancellationToken)
        {
            while (seconds > 0f)
            {
                await UniTask.Yield(cancellationToken);
                seconds -= Time.deltaTime;
            }
        }
    }
}
