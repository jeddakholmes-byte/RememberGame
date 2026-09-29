/*:
 * @plugindesc v1.0 Optional interaction hints, quiet footsteps and a story menu for RememberMe Act I.
 * @author RememberMe
 * @help
 * Install below Community_Basic. Disable RM_Act1_Polish if enabled.
 * Designed for the supplied Act I maps 1 and 2 in RPG Maker MV.
 * Does not change story flags, dialogue, portraits or event routes.
 * Event notes: <RMHint: Look at the flowers>
 * Options include Interaction Hints and Footsteps. Both default to ON.
 * Hints hide during events, messages, transfers and fades.
 * No automatic saving and no network requests.
 */
(function() {
    'use strict';
    function inActOne() {
        return typeof $gameMap !== 'undefined' && $gameMap &&
            ($gameMap.mapId() === 1 || $gameMap.mapId() === 2);
    }
    ConfigManager.rmInteractionHints = true;
    ConfigManager.rmFootsteps = true;
    var makeData = ConfigManager.makeData;
    ConfigManager.makeData = function() {
        var data = makeData.call(this);
        data.rmInteractionHints = this.rmInteractionHints;
        data.rmFootsteps = this.rmFootsteps;
        return data;
    };
    var applyData = ConfigManager.applyData;
    ConfigManager.applyData = function(data) {
        applyData.call(this, data);
        this.rmInteractionHints = data.rmInteractionHints === undefined ? true : !!data.rmInteractionHints;
        this.rmFootsteps = data.rmFootsteps === undefined ? true : !!data.rmFootsteps;
    };
    var options = Window_Options.prototype.addGeneralOptions;
    Window_Options.prototype.addGeneralOptions = function() {
        options.call(this);
        this.addCommand('Interaction Hints', 'rmInteractionHints');
        this.addCommand('Footsteps', 'rmFootsteps');
    };

    var increaseSteps = Game_Player.prototype.increaseSteps;
    Game_Player.prototype.increaseSteps = function() {
        increaseSteps.call(this);
        if (!inActOne() || !ConfigManager.rmFootsteps || !this.isNormal() ||
            this.isMoveRouteForcing() || $gameMap.isEventRunning() || $gameMessage.isBusy()) return;
        this._rmStepCount = (this._rmStepCount || 0) + 1;
        if (this._rmStepCount % 2 === 0) {
            AudioManager.playSe({name: 'RM_Step' + (1 + this._rmStepCount % 3), volume: 12, pitch: 100, pan: 0});
        }
    };

    function hintText() {
        if (!inActOne() || !ConfigManager.rmInteractionHints || !$gameSwitches.value(1) ||
            $gameSwitches.value(4) || $gameMap.isEventRunning() || $gameMessage.isBusy() ||
            $gamePlayer.isTransferring() || $gameScreen.brightness() < 255 ||
            $gamePlayer.isMoving()) return '';
        var d = $gamePlayer.direction();
        var x = $gameMap.roundXWithDirection($gamePlayer.x, d);
        var y = $gameMap.roundYWithDirection($gamePlayer.y, d);
        var events = $gameMap.eventsXy(x, y);
        for (var i = 0; i < events.length; i++) {
            var event = events[i];
            if (event._erased || !event.page() || !event.isNormalPriority() || event._trigger !== 0) continue;
            var match = /<RMHint:\s*([^>]+)>/i.exec(event.event().note || '');
            if (match) return 'Enter / Space: ' + match[1];
        }
        return '';
    }
    var createDisplayObjects = Scene_Map.prototype.createDisplayObjects;
    Scene_Map.prototype.createDisplayObjects = function() {
        createDisplayObjects.call(this);
        this._rmHint = new Sprite(new Bitmap(580, 36));
        this._rmHint.x = Math.round((Graphics.boxWidth - 580) / 2);
        this._rmHint.y = Graphics.boxHeight - 48;
        this._rmHint.visible = false;
        this._rmHintText = '';
        this.addChild(this._rmHint);
    };
    var updateMap = Scene_Map.prototype.update;
    Scene_Map.prototype.update = function() {
        updateMap.call(this);
        if (!this._rmHint) return;
        var text = hintText();
        this._rmHint.visible = !!text && this.isActive();
        if (text !== this._rmHintText) {
            this._rmHintText = text;
            var bitmap = this._rmHint.bitmap;
            bitmap.clear();
            if (text) {
                bitmap.fillRect(0, 0, 580, 36, 'rgba(12,16,18,0.78)');
                bitmap.fontSize = 20;
                bitmap.textColor = '#eee9df';
                bitmap.drawText(text, 10, 0, 560, 36, 'center');
            }
        }
    };

    var menuList = Window_MenuCommand.prototype.makeCommandList;
    Window_MenuCommand.prototype.makeCommandList = function() {
        if (!inActOne()) return menuList.call(this);
        this.addCommand('Resume', 'cancel');
        this.addSaveCommand();
        this.addOptionsCommand();
        this.addGameEndCommand();
    };
    var menuCreate = Scene_Menu.prototype.create;
    Scene_Menu.prototype.create = function() {
        if (!inActOne()) return menuCreate.call(this);
        Scene_MenuBase.prototype.create.call(this);
        this.createCommandWindow();
        this._commandWindow.x = Math.round((Graphics.boxWidth - this._commandWindow.width) / 2);
        this._commandWindow.y = Math.round((Graphics.boxHeight - this._commandWindow.height) / 2);
    };
    var menuStart = Scene_Menu.prototype.start;
    Scene_Menu.prototype.start = function() {
        if (!inActOne()) return menuStart.call(this);
        Scene_MenuBase.prototype.start.call(this);
    };
})();
