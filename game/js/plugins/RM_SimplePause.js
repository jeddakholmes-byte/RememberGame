/*:
 * @plugindesc Simple Menu
 * @author Remember Me
 */

(function() {
    "use strict";

    var originalCreate = Scene_Menu.prototype.create;

    Scene_Menu.prototype.create = function() {
        originalCreate.call(this);

        this._goldWindow.hide();
        this._statusWindow.hide();

        var menu = this._commandWindow;
        menu.x = Math.round((Graphics.boxWidth - menu.width) / 2);
        menu.y = Math.round((Graphics.boxHeight - menu.height) / 2);
    };
})();