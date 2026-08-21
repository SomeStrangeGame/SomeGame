using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
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
                        SetImageImmediate = location.SetImageImmediate,
                        SetCamera = location.SetCamera,
                        SetCameraImmediate = location.SetCameraImmediate,
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
                        SetDialogueImmediate = location.SetDialogueImmediate,
                    },
                    Bubble = new StoryQueue.StoryQueueBuilder.BubblePort
                    {
                        BubbleShow = bubble.Show,
                        BubbleShowImmediate = bubble.ShowImmediate,
                        BubbleHide = bubble.Hide,
                        BubbleHideImmediate = bubble.HideImmediate,
                        SetBubbleScreen = bubble.SetBubbleScreen,
                    },
                    Wardrobe = new StoryQueue.StoryQueueBuilder.WardrobePort
                    {
                        Show = wardrobe.Show,
                        ShowImmediate = wardrobe.ShowImmediate,
                        Hide = wardrobe.Hide,
                        HideImmediate = wardrobe.HideImmediate,
                        SetScreen = wardrobe.SetScreen,
                    },
                    Choose = new StoryQueue.StoryQueueBuilder.ChoosePort
                    {
                        Show = choose.Show,
                        ShowImmediate = choose.ShowImmediate,
                        Hide = choose.Hide,
                        HideImmediate = choose.HideImmediate,
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
                        CharacterHideImmediate = character.HideImmediate,
                        CharacterShow = character.Show,
                        CharacterShowImmediate = character.ShowImmediate,
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
