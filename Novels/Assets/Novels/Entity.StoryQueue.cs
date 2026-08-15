using System.Linq;

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
            Localization.Entity localization,
            Bubble.Entity bubble,
            Save.Entity save,
            Character.Entity character)
        {
            return new StoryQueue.Entity(new StoryQueue.Entity.Ctx
            {
                Command = new StoryQueue.Entity.CommandCtx
                {
                    ShowNotification = notification.Show,

                    SetImage = location.SetImage,
                    SetImageImmediate = location.SetImageImmediate,
                    SetCamera = location.SetCamera,
                    SetCameraImmediate = location.SetCameraImmediate,
                    Wait = waiting.Await,

                    PlayMusic = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Music),
                    PlaySound = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Sound),
                    PlayAmbient = assetName => audio.PlayAudio(assetName, Audio.Entity.Audio.Ambient),
                },

                Dialogue = new StoryQueue.Entity.DialogueCtx
                {
                    MainCharacter = _ctx.Data.MainCharacter,

                    SetDialogue = location.SetDialogue,
                    SetDialogueImmediate = location.SetDialogueImmediate,

                    GetLocalizationValue = localization.GetValue,

                    BubbleShow = bubble.Show,
                    BubbleShowImmediate = bubble.ShowImmediate,
                    BubbleHide = bubble.Hide,
                    BubbleHideImmediate = bubble.HideImmediate,
                    SetBubbleScreen = data =>
                    {
                        bubble.SetBubbleScreen(new Bubble.Entity.BubbleScreenCtx
                        {
                            Name = data.Name,
                            SpeakerRole = data.SpeakerRole,
                            Presentation = data.Presentation,
                            Text = new Bubble.Entity.BubbleScreenCtx.TextCtx
                            {
                                Header = data.Text.Header,
                                Text = data.Text.Text,
                            },
                            Buttons = data.Buttons.Select(button => new Bubble.Entity.BubbleScreenCtx.ButtonCtx
                            {
                                Id = button.Id,
                                Text = button.Text,
                                OnClick = button.OnClick
                            }).ToArray(),
                            OnBackgroundClick = data.OnBackgroundClick
                        });
                    },
                    SetWardrobeScreen = data =>
                    {
                        bubble.SetWardrobeScreen(new Bubble.Entity.WardrobeScreenCtx
                        {
                            //migrate wardrobe here...
                        });
                    },
                    SetChooseScreen = data =>
                    {
                        bubble.SetChooseScreen(new Bubble.Entity.ChooseScreenCtx
                        {
                            // migrate choose here...
                        });
                    },

                    SaveChoice = save.SaveChoice,
                    SetChoice = storyProcessor.SetChoice,

                    SetMainCharacterView = character.SetMainCharacterView,
                    SetMainCharacterClothes = character.SetMainCharacterClothes,
                    SetMainCharacterHair = character.SetMainCharacterHair,
                    CharacterHide = character.Hide,
                    CharacterHideImmediate = character.HideImmediate,
                    CharacterShow = character.Show,
                    CharacterShowImmediate = character.ShowImmediate,
                    CharacterSetImage = character.SetImage,
                },
            });
        }
    }
}
