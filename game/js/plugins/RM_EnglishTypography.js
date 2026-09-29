/*:
 * @plugindesc v1.0 Select an English window font independently of the project's locale.
 * @author RememberMe
 * @param FontFace
 * @text Font Family
 * @type string
 * @default Arial, Helvetica, sans-serif
 * @desc Use GameFont to use the font configured in fonts/gamefont.css.
 *
 * @param FontSize
 * @text Font Size
 * @type number
 * @min 18
 * @max 32
 * @default 26
 *
 * @param OutlineWidth
 * @text Text Outline Width
 * @type number
 * @min 0
 * @max 4
 * @default 2
 *
 * @help
 * Place below other font or message plugins. RPG Maker MV only.
 * Overrides the standard window font, size and outline after locale selection.
 * Font size 26 is a starting suggestion, not a guaranteed fit for every line.
 * Keeps the default line height and portrait size.
 * Does not alter text painted into pictures or title backgrounds.
 * Does not bundle or download any third-party font.
 * Restart the game after changing fonts or plugin parameters.
 */
(function() {
    'use strict';
    var params = PluginManager.parameters('RM_EnglishTypography');
    var face = String(params.FontFace || 'Arial, Helvetica, sans-serif');
    var size = Math.max(18, Math.min(32, Number(params.FontSize) || 26));
    var outline = Number(params.OutlineWidth === undefined ? 2 : params.OutlineWidth);
    if (!isFinite(outline)) outline = 2;
    outline = Math.max(0, Math.min(4, outline));
    Window_Base.prototype.standardFontFace = function() { return face; };
    Window_Base.prototype.standardFontSize = function() { return size; };
    var reset = Window_Base.prototype.resetFontSettings;
    Window_Base.prototype.resetFontSettings = function() {
        reset.call(this);
        this.contents.outlineWidth = outline;
        this.contents.outlineColor = 'rgba(0, 0, 0, 0.65)';
    };
})();
