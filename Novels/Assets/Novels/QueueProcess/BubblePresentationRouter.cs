namespace Novels.QueueProcess
{
    internal sealed class BubblePresentationRouter
    {
        private readonly BubbleQueueRequest _request;
        private readonly ChoiceSelectionHandler _choices;

        internal BubblePresentationRouter(
            BubbleQueueRequest request,
            ChoiceSelectionHandler choices)
        {
            _request = request;
            _choices = choices;
        }

        internal void Present()
        {
            switch (_request.PresentationKind)
            {
                case BubbleContracts.BubblePresentationKind.Wardrobe:
                    _request.SetWardrobeScreen(_choices.CreateWardrobePresentation());
                    return;

                case BubbleContracts.BubblePresentationKind.Choose:
                    _request.SetChooseScreen(_choices.CreateChoosePresentation());
                    return;

                default:
                    _request.SetBubbleScreen(new BubbleContracts.BubblePresentation(
                        _request.Name,
                        _request.SpeakerRole,
                        _request.Presentation,
                        new BubbleContracts.BubbleText(GetHeader(), _request.Value),
                        _choices.CreatePresentations(),
                        _choices.CompleteWithoutChoice));
                    return;
            }
        }

        private string GetHeader()
        {
            if (_request.Presentation == StoryContracts.DialoguePresentation.Disclaimer)
            {
                return BubbleContracts.BubbleHeaders.Disclaimer;
            }
            if (_request.Presentation == StoryContracts.DialoguePresentation.Hint)
                return BubbleContracts.BubbleHeaders.Hint;
            return _request.Name;
        }
    }
}
