/*:
 * @plugindesc v1.0 RememberMe Act I — optional polish: title BGM fallback + player footstep SE
 * @author RememberMe Act I delivery package
 *
 * @help
 * 第一幕可选增强（RPG Maker MV 1.5 / 1.6）。
 *
 * 本插件只做两件 MV 原生事件做不到的事：
 *   1. 标题画面强制播放本包音乐 RM_Remember；
 *      即使「数据库 → 系统 → 标题音乐」漏设，标题也不会再放默认曲目。
 *   2. 玩家每走一格播放一次随机脚步音效 RM_Step1／RM_Step2／RM_Step3，
 *      音量与音调有轻微浮动；只在玩家自行走动时触发，过场与强制移动不触发。
 *
 * 安装：
 *   把本文件放进工程的 js/plugins 目录，
 *   打开「插件管理器」双击空行 → 选择 RM_Act1_Polish → 确定 → 保存。
 *   插件顺序无要求。不需要设置任何参数。
 *
 * 不使用本插件也可以：
 *   游戏其余部分完全不依赖它。不启用时，请务必在
 *   「数据库 → 系统 → 标题音乐」里手动选择 RM_Remember（音量 25）；
 *   走路则不会有脚步音效。
 *
 * 卸载：在插件管理器里把本行删除或取消勾选即可，不会影响存档与开关。
 *
 * 说明：本文件没有使用任何第三方库，也没有改写音频播放以外的方法。
 */

(function() {
    'use strict';

    // 1. 标题画面音乐
    var TITLE_BGM = { name: 'RM_Remember', volume: 25, pitch: 100, pan: 0 };
    if (typeof Scene_Title !== 'undefined' && Scene_Title.prototype.playTitleMusic) {
        Scene_Title.prototype.playTitleMusic = function() {
            AudioManager.playBgm(TITLE_BGM);
        };
    }

    // 2. 脚步音效
    var STEP_SE = ['RM_Step1', 'RM_Step2', 'RM_Step3'];
    var STEP_VOLUME = 22;

    var proto = null;
    if (typeof Game_Player !== 'undefined' && Game_Player.prototype.increaseSteps) {
        proto = Game_Player.prototype;
    } else if (typeof Game_Character !== 'undefined' && Game_Character.prototype.increaseSteps) {
        proto = Game_Character.prototype;
    }
    if (proto) {
        var _increaseSteps = proto.increaseSteps;
        proto.increaseSteps = function() {
            _increaseSteps.call(this);
            try {
                if (typeof $gamePlayer === 'undefined' || this !== $gamePlayer) return;
                if (!$gamePlayer.isNormal()) return;
                if (typeof $gameMessage !== 'undefined' && $gameMessage.isBusy()) return;
                AudioManager.playSe({
                    name: STEP_SE[Math.floor(Math.random() * STEP_SE.length)],
                    volume: STEP_VOLUME,
                    pitch: 100 + Math.floor(Math.random() * 13) - 6,
                    pan: 0
                });
            } catch (e) {
                // 音效失败不应该中断游戏流程
            }
        };
    }
})();
