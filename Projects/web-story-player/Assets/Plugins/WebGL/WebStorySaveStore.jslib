mergeInto(LibraryManager.library, {
  SomeGameWebSave: function (targetPtr, idPtr, operationPtr, keyPtr, valuePtr) {
    var target = UTF8ToString(targetPtr);
    var id = UTF8ToString(idPtr);
    var operation = UTF8ToString(operationPtr);
    var key = UTF8ToString(keyPtr);
    var value = UTF8ToString(valuePtr);
    var finished = false;
    var db;
    var reply = function (error, result) {
      if (finished) return;
      finished = true;
      if (db) db.close();
      SendMessage(target, 'OnSaveResult', JSON.stringify({
        id: id, value: result === undefined ? null : result,
        found: result !== undefined && result !== null,
        error: error ? String(error.message || error) : ''
      }));
    };
    try {
      var opening = indexedDB.open('somegame-story-saves', 1);
      opening.onupgradeneeded = function () {
        opening.result.createObjectStore('states');
      };
      opening.onerror = function () { reply(opening.error); };
      opening.onblocked = function () { reply('Save database is blocked by another tab.'); };
      opening.onsuccess = function () {
        db = opening.result;
        if (finished) { db.close(); return; }
        try {
          var tx = db.transaction('states', operation === 'read' ? 'readonly' : 'readwrite');
          var store = tx.objectStore('states');
          var result;
          var request;
          if (operation === 'read') request = store.get(key);
          else if (operation === 'write') request = store.put(value, key);
          else if (operation === 'delete') request = store.delete(key);
          else { tx.abort(); reply('Invalid save operation'); return; }
          request.onsuccess = function () {
            if (operation === 'read') result = request.result;
          };
          tx.oncomplete = function () { reply(null, result); };
          tx.onabort = function () { reply(tx.error || 'Save transaction aborted'); };
          tx.onerror = function () { reply(tx.error || 'Save transaction failed'); };
        } catch (error) { reply(error); }
      };
    } catch (error) { reply(error); }
  }
});
