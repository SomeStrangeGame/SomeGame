namespace Novels
{
    internal partial class Entity
    {
        private StoryQueue.Entity CreateStoryQueue(
            StoryProcessor.Entity storyProcessor,
            Notification.Entity notification,
            Location.Entity location,
            Waiting.Entity waiting,
            Audio.Entity audio,
            Bubble.Entity bubble,
            Wardrobe.Entity wardrobe,
            Save.Entity save,
            Character.Entity character)
        {
            return new StoryQueue.Entity(new StoryQueue.Entity.Ctx
            {
                Command = new StoryQueue.Entity.CommandCtx
                {
                    ShowNotification = notification.Enqueue,
                    Location = new StoryQueue.Entity.LocationCommandPort
                    {
                        SetImage = location.SetImage,
                        SetImageImmediate = location.SetImageImmediate,
                        SetCamera = location.SetCamera,
                        SetCameraImmediate = location.SetCameraImmediate,
                        Wait = waiting.Await,
                    },
                    Audio = new StoryQueue.Entity.AudioPort
                    {
                        PlayMusic = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Music),
                        PlaySound = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Sound),
                        PlayAmbient = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Ambient),
                    },
                },

                Dialogue = new StoryQueue.Entity.DialogueCtx
                {
                    MainCharacter = _definition.MainCharacter,

                    Location = new StoryQueue.Entity.LocationDialoguePort
                    {
                        SetDialogue = location.SetDialogue,
                        SetDialogueImmediate = location.SetDialogueImmediate,
                    },
                    Bubble = new StoryQueue.Entity.BubblePort
                    {
                        BubbleShow = bubble.Show,
                        BubbleShowImmediate = bubble.ShowImmediate,
                        BubbleHide = bubble.Hide,
                        BubbleHideImmediate = bubble.HideImmediate,
                        SetBubbleScreen = bubble.SetBubbleScreen,
                        SetChooseScreen = bubble.SetChooseScreen,
                    },
                    Wardrobe = new StoryQueue.Entity.WardrobePort
                    {
                        Show = wardrobe.Show,
                        ShowImmediate = wardrobe.ShowImmediate,
                        Hide = wardrobe.Hide,
                        HideImmediate = wardrobe.HideImmediate,
                        SetScreen = wardrobe.SetScreen,
                    },
                    Choice = new StoryQueue.Entity.ChoicePort
                    {
                        SaveDecision = save.SaveDecision,
                        SetChoice = storyProcessor.SetChoice,
                    },
                    Character = new StoryQueue.Entity.CharacterPort
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
    }
}
