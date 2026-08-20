window.lexicon = {
    dotNetHelper: null,

    init: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;

        // Add keyboard event listener
        document.addEventListener('keydown', this.handleKeyDown.bind(this));
    },

    handleKeyDown: function (e) {
        if (!this.dotNetHelper) return;

        // Prevent default behavior for keys we handle
        if (e.key === 'Enter' || e.key === 'Backspace' || /^[a-zA-Z]$/.test(e.key)) {
            e.preventDefault();
            this.dotNetHelper.invokeMethodAsync('HandleKeyPress', e.key);
        }
    },

    dispose: function () {
        document.removeEventListener('keydown', this.handleKeyDown.bind(this));
        this.dotNetHelper = null;
    }
};
