mergeInto(LibraryManager.library, {
  SomeGameWebPlayerEmit: function (jsonPointer) {
    var detail;
    try {
      detail = JSON.parse(UTF8ToString(jsonPointer));
    } catch (error) {
      detail = { type: 'player_error', code: 'invalid_unity_event' };
    }

    window.dispatchEvent(new CustomEvent('somegame:web-player-event', {
      detail: detail
    }));
  }
});
