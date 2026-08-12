mergeInto(LibraryManager.library, {
  RPlayGames_GetInjectedToken: function () {
    if (!window.RplayGameSDK || !window.RplayGameSDK.getToken) {
      return 0;
    }

    var token = window.RplayGameSDK.getToken();
    if (!token) {
      return 0;
    }

    var length = lengthBytesUTF8(token) + 1;
    var pointer = _malloc(length);
    stringToUTF8(token, pointer, length);
    return pointer;
  },

  RPlayGames_Free: function (pointer) {
    if (pointer) {
      _free(pointer);
    }
  }
});
