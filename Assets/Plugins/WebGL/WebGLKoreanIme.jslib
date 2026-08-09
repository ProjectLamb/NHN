mergeInto(LibraryManager.library, {
  SandwichImeCreate: function (receiverPtr) {
    var receiver = UTF8ToString(receiverPtr);
    var state = window.__sandwichIme;

    if (!state) {
      var input = document.createElement("textarea");
      input.id = "sandwich-webgl-ime";
      input.autocomplete = "off";
      input.autocapitalize = "off";
      input.spellcheck = false;
      input.rows = 1;
      input.tabIndex = -1;
      input.inputMode = "text";
      input.lang = "ko";
      input.setAttribute("aria-label", "Game command input");
      input.style.position = "fixed";
      input.style.left = "50%";
      input.style.bottom = "8px";
      input.style.width = "2px";
      input.style.height = "2px";
      input.style.opacity = "0.01";
      input.style.zIndex = "2147483647";
      input.style.resize = "none";
      input.style.pointerEvents = "none";
      input.style.border = "0";
      input.style.padding = "0";
      input.style.overflow = "hidden";
      document.body.appendChild(input);

      state = window.__sandwichIme = {
        input: input,
        receiver: receiver,
        composing: false,
        wantsFocus: false
      };

      state.getFullscreenElement = function () {
        return document.fullscreenElement ||
          document.webkitFullscreenElement ||
          document.mozFullScreenElement ||
          document.msFullscreenElement ||
          null;
      };

      state.moveInputToActiveRoot = function () {
        var root = state.getFullscreenElement() || document.body;
        if (state.input.parentNode !== root) {
          root.appendChild(state.input);
        }
      };

      state.focusInput = function () {
        state.moveInputToActiveRoot();
        try {
          state.input.focus({ preventScroll: true });
        } catch (error) {
          state.input.focus();
        }
        var end = state.input.value.length;
        state.input.setSelectionRange(end, end);
      };

      state.onFullscreenChange = function () {
        state.moveInputToActiveRoot();
        if (!state.wantsFocus || !state.receiver) return;

        // Fullscreen swaps can temporarily blur the DOM input. Refocus after
        // the browser has installed the fullscreen element in the top layer.
        window.setTimeout(state.focusInput, 0);
        window.setTimeout(state.focusInput, 80);
      };

      document.addEventListener("fullscreenchange", state.onFullscreenChange);
      document.addEventListener("webkitfullscreenchange", state.onFullscreenChange);
      document.addEventListener("mozfullscreenchange", state.onFullscreenChange);
      document.addEventListener("MSFullscreenChange", state.onFullscreenChange);

      input.addEventListener("compositionstart", function () {
        state.composing = true;
      });

      input.addEventListener("compositionend", function () {
        state.composing = false;
        SendMessage(state.receiver, "OnWebGLImeInput", input.value);
      });

      input.addEventListener("input", function () {
        SendMessage(state.receiver, "OnWebGLImeInput", input.value);
      });

      input.addEventListener("keydown", function (event) {
        if (event.key === "Enter" && !state.composing && !event.isComposing) {
          event.preventDefault();
          SendMessage(state.receiver, "OnWebGLImeSubmit", "");
        }
      });
    }

    state.receiver = receiver;
  },

  SandwichImeFocus: function (receiverPtr, valuePtr) {
    var state = window.__sandwichIme;
    if (!state) return;

    state.receiver = UTF8ToString(receiverPtr);
    state.input.value = UTF8ToString(valuePtr);
    state.wantsFocus = true;
    state.focusInput();
  },

  SandwichImeSetValue: function (valuePtr) {
    var state = window.__sandwichIme;
    if (!state) return;
    state.input.value = UTF8ToString(valuePtr);
  },

  SandwichImeDestroy: function (receiverPtr) {
    var state = window.__sandwichIme;
    if (!state || state.receiver !== UTF8ToString(receiverPtr)) return;
    state.wantsFocus = false;
    state.input.blur();
    state.receiver = "";
  }
});
