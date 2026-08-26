using System;

namespace Novels.Catalog
{
    public sealed class CatalogAction
    {
        private readonly Action _execute;

        public CatalogAction(string text, Action execute)
        {
            Text = text ?? string.Empty;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            IsInteractable = true;
        }

        public event Action Changed;

        public string Text { get; private set; }
        public bool IsInteractable { get; private set; }

        public void SetState(string text, bool isInteractable)
        {
            text ??= string.Empty;
            if (string.Equals(Text, text, StringComparison.Ordinal)
                && IsInteractable == isInteractable)
            {
                return;
            }
            Text = text;
            IsInteractable = isInteractable;
            Changed?.Invoke();
        }

        public void Invoke()
        {
            if (IsInteractable)
                _execute();
        }
    }
}
