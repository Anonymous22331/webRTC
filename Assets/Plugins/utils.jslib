mergeInto(LibraryManager.library, {
  sendImageToDesktop: function (url) {
    window.open(UTF8ToString(url), '_blank');
  }
});