namespace Novels.StoryExecution
{
    internal sealed class BubblePresentationRouter
    {
        private readonly BubbleOperationRequest _request;
        private readonly ChoiceSelectionHandler _choices;

        internal BubblePresentationRouter(
            BubbleOperationRequest request,
            ChoiceSelectionHandler choices)
        {
            _request = request;
            _choices = choices;
        }

        internal void Present()
        {
            _request.Services.OnDialogueReady?.Invoke(
                _request.PresentationKind.ToString(),
                _request.Choices.Length);
            switch (_request.PresentationKind)
            {
                case BubbleContracts.BubblePresentationKind.Wardrobe:
                    _request.Services.Wardrobe.SetScreen(
                        _choices.CreateWardrobePresentation());
                    return;

                case BubbleContracts.BubblePresentationKind.Choose:
                    _request.Services.Choose.SetScreen(
                        _choices.CreateChoosePresentation());
                    return;

                default:
                    _request.Services.Bubble.SetBubbleScreen(
                        new BubbleContracts.BubblePresentation(
                        DisplayName,
                        _request.SpeakerRole,
                        _request.Dialogue.Presentation,
                        new BubbleContracts.BubbleText(
                            GetHeader(), _request.Dialogue.Text),
                        _choices.CreatePresentations(),
                        _choices.CompleteWithoutChoice));
                    return;
            }
        }

        private string GetHeader()
        {
            if (_request.Dialogue.Presentation
                == StoryContracts.DialoguePresentation.Disclaimer)
            {
                return BubbleContracts.BubbleHeaders.Disclaimer;
            }
            if (_request.Dialogue.Presentation == StoryContracts.DialoguePresentation.Hint)
                return BubbleContracts.BubbleHeaders.Hint;
            return DisplayName;
        }

        private string DisplayName =>
            string.IsNullOrEmpty(_request.Dialogue.Character.DisplayName)
                ? _request.Dialogue.Speaker
                : _request.Dialogue.Character.DisplayName;
    }
}
