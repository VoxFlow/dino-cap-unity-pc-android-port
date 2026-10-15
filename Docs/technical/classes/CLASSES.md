# C++ classes and virtual tables

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

Virtual tables found from the binary's mangled symbols (`__ZTV...`): 81. The `_ZTV` symbol points at the offset-to-top word; the typeinfo pointer follows and the first virtual function pointer is at symbol+8 (the vptr stored in objects points there). The recovered rules use both conventions: `mainSceneiPad` slot +0x38 is measured from the symbol (0x1FDAD0), the player table's +0x24 from the vptr (0x1FDCB8 = symbol 0x1FDCB0 + 8). Both offsets are listed. Import-pointer cells (`..._ptr` symbols) are not tables and are excluded. Slot targets are function addresses; a slot with a dossier is linked. Virtual calls are why many handlers show no direct callers.

## `vtable for'UIIAP`

Vtable at `0x1FD6B0` (symbol `_ZTV5UIIAP`), 29 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xBDE8` (`sub_BDE8`) | indexed |
| 1 | +0xC / +0x4 | `0xBD48` (`sub_BD48`) | indexed |
| 2 | +0x10 / +0x8 | `0xBF34` (`sub_BF34`) | indexed |
| 3 | +0x14 / +0xC | `0x18324C` (`sub_18324C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x183414` (`sub_183414`) | indexed |
| 22 | +0x60 / +0x58 | `0x183428` (`sub_183428`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0xC650` (`sub_C650`) | indexed |
| 26 | +0x70 / +0x68 | `0xC990` (`sub_C990`) | indexed |
| 27 | +0x74 / +0x6C | `0xC698` (`sub_C698`) | indexed |
| 28 | +0x78 / +0x70 | `0xC700` (`sub_C700`) | indexed |

## `vtable for'DinoCap::houseClassiPad`

Vtable at `0x1FD780` (symbol `_ZTVN7DinoCap14houseClassiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xCD54` (`sub_CD54`) | indexed |
| 1 | +0xC / +0x4 | `0xCCB4` (`sub_CCB4`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0xCE24` (`sub_CE24`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x76A20` (`sub_76A20`) | [dossier](../functions/dossiers/animation-rendering/FUN-076A20.md) |
| 7 | +0x24 / +0x1C | `0xD004` (`sub_D004`) | [dossier](../functions/dossiers/doors-buildings/FUN-00D004.md) |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0xCFE8` (`sub_CFE8`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0xD108` (`sub_D108`) | [dossier](../functions/dossiers/doors-buildings/FUN-00D108.md) |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::bglayerClassiPad`

Vtable at `0x1FD7C0` (symbol `_ZTVN7DinoCap16bglayerClassiPadE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD21C` (`sub_D21C`) | indexed |
| 1 | +0xC / +0x4 | `0xD17C` (`sub_D17C`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xD308` (`sub_D308`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::bulletClassiPad`

Vtable at `0x1FD810` (symbol `_ZTVN7DinoCap15bulletClassiPadE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD4D0` (`sub_D4D0`) | indexed |
| 1 | +0xC / +0x4 | `0xD430` (`sub_D430`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xD5B0` (`sub_D5B0`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::particleClassiPad`

Vtable at `0x1FD840` (symbol `_ZTVN7DinoCap17particleClassiPadE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD9B8` (`sub_D9B8`) | indexed |
| 1 | +0xC / +0x4 | `0xD918` (`sub_D918`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xDAA0` (`sub_DAA0`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::itemClassiPad`

Vtable at `0x1FD880` (symbol `_ZTVN7DinoCap13itemClassiPadE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xEB54` (`sub_EB54`) | indexed |
| 1 | +0xC / +0x4 | `0xEAB4` (`sub_EAB4`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0xEC70` (`sub_EC70`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xEFB4` (`sub_EFB4`) | [dossier](../functions/dossiers/doors-buildings/FUN-00EFB4.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::dinosaurClassiPad`

Vtable at `0x1FD8D0` (symbol `_ZTVN7DinoCap17dinosaurClassiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xFBF0` (`sub_FBF0`) | indexed |
| 1 | +0xC / +0x4 | `0xFB50` (`sub_FB50`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xFD58` (`sub_FD58`) | [dossier](../functions/dossiers/animation-rendering/FUN-00FD58.md) |
| 7 | +0x24 / +0x1C | `0x12D00` (`sub_12D00`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x101A0` (`sub_101A0`) | [dossier](../functions/dossiers/input-combat/FUN-0101A0.md) |
| 10 | +0x30 / +0x28 | `0x10220` (`sub_10220`) | indexed |
| 11 | +0x34 / +0x2C | `0x10294` (`sub_10294`) | [dossier](../functions/dossiers/input-combat/FUN-010294.md) |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::effectClassiPad`

Vtable at `0x1FD920` (symbol `_ZTVN7DinoCap15effectClassiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x12E60` (`sub_12E60`) | indexed |
| 1 | +0xC / +0x4 | `0x12DC0` (`sub_12DC0`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x13174` (`sub_13174`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x12F28` (`sub_12F28`) | [dossier](../functions/dossiers/input-combat/FUN-012F28.md) |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::baozhaClassiPad`

Vtable at `0x1FD960` (symbol `_ZTVN7DinoCap15baozhaClassiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13604` (`sub_13604`) | indexed |
| 1 | +0xC / +0x4 | `0x13564` (`sub_13564`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x76A20` (`sub_76A20`) | [dossier](../functions/dossiers/animation-rendering/FUN-076A20.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x136BC` (`sub_136BC`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::lifeModeDuiPad`

Vtable at `0x1FD9C0` (symbol `_ZTVN7DinoCap14lifeModeDuiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13910` (`sub_13910`) | indexed |
| 1 | +0xC / +0x4 | `0x13870` (`sub_13870`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x1383C` (`sub_1383C`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x13B10` (`sub_13B10`) | indexed |
| 7 | +0x24 / +0x1C | `0x13D60` (`nullsub_2`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x13D64` (`sub_13D64`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::lifeModeSceneiPad`

Vtable at `0x1FDA10` (symbol `_ZTVN7DinoCap17lifeModeSceneiPadE`), 22 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13E88` (`sub_13E88`) | indexed |
| 1 | +0xC / +0x4 | `0x13DE8` (`sub_13DE8`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x13FD4` (`sub_13FD4`) | [dossier](../functions/dossiers/input-combat/FUN-013FD4.md) |
| 5 | +0x1C / +0x14 | `0x17AB4` (`sub_17AB4`) | indexed |
| 6 | +0x20 / +0x18 | `0x162AC` (`sub_162AC`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x17650` (`sub_17650`) | indexed |
| 9 | +0x2C / +0x24 | `0x17AE4` (`sub_17AE4`) | [dossier](../functions/dossiers/input-combat/FUN-017AE4.md) |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x13FC8` (`sub_13FC8`) | indexed |
| 12 | +0x38 / +0x30 | `0x1802C` (`sub_1802C`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0x58954` (`sub_58954`) | indexed |
| 15 | +0x44 / +0x3C | `0x19920` (`__ZN7DinoCap17lifeModeSceneiPad9UpdateExpEv`) | indexed |
| 16 | +0x48 / +0x40 | `0x19340` (`sub_19340`) | indexed |
| 17 | +0x4C / +0x44 | `0x18858` (`sub_18858`) | [dossier](../functions/dossiers/doors-buildings/FUN-018858.md) |
| 18 | +0x50 / +0x48 | `0x17A08` (`sub_17A08`) | indexed |
| 19 | +0x54 / +0x4C | `0x166FC` (`sub_166FC`) | indexed |
| 20 | +0x58 / +0x50 | `0x16928` (`sub_16928`) | indexed |
| 21 | +0x5C / +0x54 | `0x16A30` (`sub_16A30`) | indexed |

## `vtable for'DinoCap::lifeModeToolsiPad`

Vtable at `0x1FDA80` (symbol `_ZTVN7DinoCap17lifeModeToolsiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x19E5C` (`sub_19E5C`) | indexed |
| 1 | +0xC / +0x4 | `0x19DBC` (`sub_19DBC`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x19D88` (`sub_19D88`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x1B13C` (`sub_1B13C`) | [dossier](../functions/dossiers/input-combat/FUN-01B13C.md) |
| 7 | +0x24 / +0x1C | `0x1BB3C` (`sub_1BB3C`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::mainSceneiPad`

Vtable at `0x1FDAD0` (symbol `_ZTVN7DinoCap13mainSceneiPadE`), 17 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x53198` (`sub_53198`) | indexed |
| 1 | +0xC / +0x4 | `0x530F8` (`sub_530F8`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x53330` (`sub_53330`) | [dossier](../functions/dossiers/doors-buildings/FUN-053330.md) |
| 5 | +0x1C / +0x14 | `0x56E40` (`sub_56E40`) | indexed |
| 6 | +0x20 / +0x18 | `0x55B1C` (`sub_55B1C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-055B1C.md) |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x56118` (`sub_56118`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-056118.md) |
| 9 | +0x2C / +0x24 | `0x56E94` (`sub_56E94`) | [dossier](../functions/dossiers/pause-time/FUN-056E94.md) |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x53324` (`sub_53324`) | indexed |
| 12 | +0x38 / +0x30 | `0x5744C` (`sub_5744C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-05744C.md) |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0x58954` (`sub_58954`) | indexed |
| 15 | +0x44 / +0x3C | `0x58B5C` (`sub_58B5C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-058B5C.md) |
| 16 | +0x48 / +0x40 | `0x5931C` (`sub_5931C`) | [dossier](../functions/dossiers/pause-time/FUN-05931C.md) |

## `vtable for'DinoCap::menuObjiPad`

Vtable at `0x1FDB30` (symbol `_ZTVN7DinoCap11menuObjiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x5999C` (`sub_5999C`) | indexed |
| 1 | +0xC / +0x4 | `0x598FC` (`sub_598FC`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x59A54` (`sub_59A54`) | [dossier](../functions/dossiers/animation-rendering/FUN-059A54.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x59AB0` (`sub_59AB0`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::menuSceneiPad`

Vtable at `0x1FDB70` (symbol `_ZTVN7DinoCap13menuSceneiPadE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x59C54` (`sub_59C54`) | indexed |
| 1 | +0xC / +0x4 | `0x59BB4` (`sub_59BB4`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x59DEC` (`sub_59DEC`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-059DEC.md) |
| 5 | +0x1C / +0x14 | `0x5D0E0` (`sub_5D0E0`) | indexed |
| 6 | +0x20 / +0x18 | `0x62A98` (`sub_62A98`) | [dossier](../functions/dossiers/engine-platform/FUN-062A98.md) |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x5D380` (`sub_5D380`) | indexed |
| 9 | +0x2C / +0x24 | `0x189E88` (`nullsub_47`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x61AD4` (`sub_61AD4`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'DinoCap::pauseCtrliPad`

Vtable at `0x1FDBD0` (symbol `_ZTVN7DinoCap13pauseCtrliPadE`), 32 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x63E80` (`sub_63E80`) | indexed |
| 1 | +0xC / +0x4 | `0x63DE0` (`sub_63DE0`) | indexed |
| 2 | +0x10 / +0x8 | `0x64694` (`sub_64694`) | [dossier](../functions/dossiers/pause-time/FUN-064694.md) |
| 3 | +0x14 / +0xC | `0x183D9C` (`sub_183D9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x64F18` (`sub_64F18`) | indexed |
| 26 | +0x70 / +0x68 | `0x65468` (`sub_65468`) | indexed |
| 27 | +0x74 / +0x6C | `0x64F48` (`sub_64F48`) | indexed |
| 28 | +0x78 / +0x70 | `0x63FDC` (`sub_63FDC`) | indexed |
| 29 | +0x7C / +0x74 | `0x63FFC` (`sub_63FFC`) | indexed |
| 30 | +0x80 / +0x78 | `0x64030` (`sub_64030`) | [dossier](../functions/dossiers/pause-time/FUN-064030.md) |
| 31 | +0x84 / +0x7C | `0x6401C` (`sub_6401C`) | indexed |

## `vtable for'DinoCap::playerClassiPad`

Vtable at `0x1FDCB0` (symbol `_ZTVN7DinoCap15playerClassiPadE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x65814` (`sub_65814`) | indexed |
| 1 | +0xC / +0x4 | `0x65774` (`sub_65774`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x65988` (`sub_65988`) | [dossier](../functions/dossiers/input-combat/FUN-065988.md) |
| 5 | +0x1C / +0x14 | `0x65C74` (`nullsub_3`) | indexed |
| 6 | +0x20 / +0x18 | `0x65C78` (`sub_65C78`) | [dossier](../functions/dossiers/input-combat/FUN-065C78.md) |
| 7 | +0x24 / +0x1C | `0x66110` (`sub_66110`) | [dossier](../functions/dossiers/rage-face/FUN-066110.md) |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x66550` (`sub_66550`) | [dossier](../functions/dossiers/input-combat/FUN-066550.md) |
| 10 | +0x30 / +0x28 | `0x663A0` (`sub_663A0`) | [dossier](../functions/dossiers/input-combat/FUN-0663A0.md) |
| 11 | +0x34 / +0x2C | `0x66C74` (`sub_66C74`) | [dossier](../functions/dossiers/input-combat/FUN-066C74.md) |
| 12 | +0x38 / +0x30 | `0x66CE8` (`sub_66CE8`) | [dossier](../functions/dossiers/input-combat/FUN-066CE8.md) |

## `vtable for'DinoCap::UILevelCtrliPad`

Vtable at `0x1FDD00` (symbol `_ZTVN7DinoCap15UILevelCtrliPadE`), 32 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x68FB8` (`sub_68FB8`) | indexed |
| 1 | +0xC / +0x4 | `0x68F18` (`sub_68F18`) | indexed |
| 2 | +0x10 / +0x8 | `0x69134` (`sub_69134`) | indexed |
| 3 | +0x14 / +0xC | `0x699FC` (`sub_699FC`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x69B0C` (`sub_69B0C`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x183414` (`sub_183414`) | indexed |
| 22 | +0x60 / +0x58 | `0x183428` (`sub_183428`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x69400` (`sub_69400`) | indexed |
| 26 | +0x70 / +0x68 | `0x69444` (`sub_69444`) | indexed |
| 27 | +0x74 / +0x6C | `0x695DC` (`sub_695DC`) | indexed |
| 28 | +0x78 / +0x70 | `0x69390` (`sub_69390`) | indexed |
| 29 | +0x7C / +0x74 | `0x693B0` (`sub_693B0`) | indexed |
| 30 | +0x80 / +0x78 | `0x693D0` (`sub_693D0`) | indexed |
| 31 | +0x84 / +0x7C | `0x693EC` (`sub_693EC`) | indexed |

## `vtable for'DinoCap::houseCtrl`

Vtable at `0x1FDDC0` (symbol `_ZTVN7DinoCap9houseCtrlE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x6A198` (`__ZN7DinoCap9houseCtrlD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0x6A19C` (`__ZN7DinoCap9houseCtrlD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x183D3C` (`sub_183D3C`) | indexed |
| 3 | +0x14 / +0xC | `0x183D9C` (`sub_183D9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x183DE4` (`sub_183DE4`) | indexed |
| 26 | +0x70 / +0x68 | `0x69F30` (`sub_69F30`) | [dossier](../functions/dossiers/doors-buildings/FUN-069F30.md) |
| 27 | +0x74 / +0x6C | `0x69FF0` (`sub_69FF0`) | [dossier](../functions/dossiers/doors-buildings/FUN-069FF0.md) |

## `vtable for'DinoCap::upgradeSceneiPad`

Vtable at `0x1FDE70` (symbol `_ZTVN7DinoCap16upgradeSceneiPadE`), 17 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x6A320` (`sub_6A320`) | indexed |
| 1 | +0xC / +0x4 | `0x6A280` (`sub_6A280`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x6A4A8` (`sub_6A4A8`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-06A4A8.md) |
| 5 | +0x1C / +0x14 | `0x6EF9C` (`sub_6EF9C`) | indexed |
| 6 | +0x20 / +0x18 | `0x6F124` (`sub_6F124`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-06F124.md) |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x6A444` (`sub_6A444`) | indexed |
| 9 | +0x2C / +0x24 | `0x6F594` (`sub_6F594`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x53324` (`sub_53324`) | indexed |
| 12 | +0x38 / +0x30 | `0x5744C` (`sub_5744C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-05744C.md) |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0x7334C` (`sub_7334C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-07334C.md) |
| 15 | +0x44 / +0x3C | `0x58B5C` (`sub_58B5C`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-058B5C.md) |
| 16 | +0x48 / +0x40 | `0x5931C` (`sub_5931C`) | [dossier](../functions/dossiers/pause-time/FUN-05931C.md) |

## `vtable for'DinoCap::houseClass`

Vtable at `0x1FDED0` (symbol `_ZTVN7DinoCap10houseClassE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x73740` (`sub_73740`) | indexed |
| 1 | +0xC / +0x4 | `0x736A0` (`sub_736A0`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x73810` (`sub_73810`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x76A20` (`sub_76A20`) | [dossier](../functions/dossiers/animation-rendering/FUN-076A20.md) |
| 7 | +0x24 / +0x1C | `0x73B90` (`sub_73B90`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x73B74` (`sub_73B74`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x73C94` (`sub_73C94`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::bglayerClass`

Vtable at `0x1FDF10` (symbol `_ZTVN7DinoCap12bglayerClassE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x73DA8` (`sub_73DA8`) | indexed |
| 1 | +0xC / +0x4 | `0x73D08` (`sub_73D08`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x73E94` (`sub_73E94`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::bulletClass`

Vtable at `0x1FDF60` (symbol `_ZTVN7DinoCap11bulletClassE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x7405C` (`sub_7405C`) | indexed |
| 1 | +0xC / +0x4 | `0x73FBC` (`sub_73FBC`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x74134` (`sub_74134`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::particleClass`

Vtable at `0x1FDF90` (symbol `_ZTVN7DinoCap13particleClassE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x7453C` (`sub_7453C`) | indexed |
| 1 | +0xC / +0x4 | `0x7449C` (`sub_7449C`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x7461C` (`sub_7461C`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::itemClass`

Vtable at `0x1FDFD0` (symbol `_ZTVN7DinoCap9itemClassE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x756CC` (`sub_756CC`) | indexed |
| 1 | +0xC / +0x4 | `0x7562C` (`sub_7562C`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x757E8` (`sub_757E8`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x75B24` (`sub_75B24`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'DinoCap::CAObjCallBack`

Vtable at `0x1FE020` (symbol `_ZTVN7DinoCap13CAObjCallBackE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x76738` (`sub_76738`) | indexed |
| 1 | +0xC / +0x4 | `0x76698` (`sub_76698`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x76A20` (`sub_76A20`) | [dossier](../functions/dossiers/animation-rendering/FUN-076A20.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::dinosaurClass`

Vtable at `0x1FE070` (symbol `_ZTVN7DinoCap13dinosaurClassE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x79A0C` (`sub_79A0C`) | indexed |
| 1 | +0xC / +0x4 | `0x7996C` (`sub_7996C`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x79B74` (`sub_79B74`) | [dossier](../functions/dossiers/animation-rendering/FUN-079B74.md) |
| 7 | +0x24 / +0x1C | `0x7CAF8` (`sub_7CAF8`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x79FBC` (`sub_79FBC`) | indexed |
| 10 | +0x30 / +0x28 | `0x7A03C` (`sub_7A03C`) | indexed |
| 11 | +0x34 / +0x2C | `0x7A0B0` (`sub_7A0B0`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::effectClass`

Vtable at `0x1FE0C0` (symbol `_ZTVN7DinoCap11effectClassE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x7CC58` (`sub_7CC58`) | indexed |
| 1 | +0xC / +0x4 | `0x7CBB8` (`sub_7CBB8`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x7CF6C` (`sub_7CF6C`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x7CD20` (`sub_7CD20`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::baozhaClass`

Vtable at `0x1FE100` (symbol `_ZTVN7DinoCap11baozhaClassE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x7D3F8` (`sub_7D3F8`) | indexed |
| 1 | +0xC / +0x4 | `0x7D358` (`sub_7D358`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x76A20` (`sub_76A20`) | [dossier](../functions/dossiers/animation-rendering/FUN-076A20.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x7D4B0` (`sub_7D4B0`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::lifeModeDu`

Vtable at `0x1FE160` (symbol `_ZTVN7DinoCap10lifeModeDuE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x80F48` (`sub_80F48`) | indexed |
| 1 | +0xC / +0x4 | `0x80EA8` (`sub_80EA8`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x80E74` (`sub_80E74`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x81158` (`sub_81158`) | indexed |
| 7 | +0x24 / +0x1C | `0x813A8` (`nullsub_8`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x813AC` (`sub_813AC`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::lifeModeScene`

Vtable at `0x1FE1B0` (symbol `_ZTVN7DinoCap13lifeModeSceneE`), 22 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x814D0` (`sub_814D0`) | indexed |
| 1 | +0xC / +0x4 | `0x81430` (`sub_81430`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x8161C` (`sub_8161C`) | [dossier](../functions/dossiers/input-combat/FUN-08161C.md) |
| 5 | +0x1C / +0x14 | `0x85210` (`sub_85210`) | indexed |
| 6 | +0x20 / +0x18 | `0x84A20` (`sub_84A20`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x84E5C` (`sub_84E5C`) | indexed |
| 9 | +0x2C / +0x24 | `0x85240` (`sub_85240`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x81610` (`sub_81610`) | indexed |
| 12 | +0x38 / +0x30 | `0x85788` (`sub_85788`) | [dossier](../functions/dossiers/other/FUN-085788.md) |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0xC7DDC` (`sub_C7DDC`) | indexed |
| 15 | +0x44 / +0x3C | `0x86CCC` (`__ZN7DinoCap13lifeModeScene9UpdateExpEv`) | indexed |
| 16 | +0x48 / +0x40 | `0x86A98` (`sub_86A98`) | indexed |
| 17 | +0x4C / +0x44 | `0x85FB0` (`sub_85FB0`) | indexed |
| 18 | +0x50 / +0x48 | `0x83A48` (`sub_83A48`) | indexed |
| 19 | +0x54 / +0x4C | `0x83AF4` (`sub_83AF4`) | indexed |
| 20 | +0x58 / +0x50 | `0x83CFC` (`sub_83CFC`) | indexed |
| 21 | +0x5C / +0x54 | `0x83E04` (`sub_83E04`) | indexed |

## `vtable for'DinoCap::lifeModeTools`

Vtable at `0x1FE220` (symbol `_ZTVN7DinoCap13lifeModeToolsE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x870B0` (`sub_870B0`) | indexed |
| 1 | +0xC / +0x4 | `0x87010` (`sub_87010`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x86FDC` (`sub_86FDC`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x88360` (`sub_88360`) | indexed |
| 7 | +0x24 / +0x1C | `0x88CA0` (`sub_88CA0`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0x77448` (`nullsub_6`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::mainScene`

Vtable at `0x1FE270` (symbol `_ZTVN7DinoCap9mainSceneE`), 17 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xC28D8` (`sub_C28D8`) | indexed |
| 1 | +0xC / +0x4 | `0xC2838` (`sub_C2838`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xC2A70` (`sub_C2A70`) | indexed |
| 5 | +0x1C / +0x14 | `0xC62B0` (`sub_C62B0`) | indexed |
| 6 | +0x20 / +0x18 | `0xC5100` (`sub_C5100`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0xC5600` (`sub_C5600`) | indexed |
| 9 | +0x2C / +0x24 | `0xC6304` (`sub_C6304`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0xC2A64` (`sub_C2A64`) | indexed |
| 12 | +0x38 / +0x30 | `0xC68D4` (`sub_C68D4`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0xC7DDC` (`sub_C7DDC`) | indexed |
| 15 | +0x44 / +0x3C | `0xC7FE8` (`sub_C7FE8`) | indexed |
| 16 | +0x48 / +0x40 | `0xC8798` (`sub_C8798`) | indexed |

## `vtable for'DinoCap::menuObj`

Vtable at `0x1FE2D0` (symbol `_ZTVN7DinoCap7menuObjE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xC8D80` (`sub_C8D80`) | indexed |
| 1 | +0xC / +0x4 | `0xC8CE0` (`sub_C8CE0`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0xC8E38` (`sub_C8E38`) | [dossier](../functions/dossiers/animation-rendering/FUN-0C8E38.md) |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0x77440` (`nullsub_4`) | indexed |
| 10 | +0x30 / +0x28 | `0x77444` (`nullsub_5`) | indexed |
| 11 | +0x34 / +0x2C | `0xC8E94` (`sub_C8E94`) | indexed |
| 12 | +0x38 / +0x30 | `0x7744C` (`nullsub_7`) | indexed |

## `vtable for'DinoCap::menuScene`

Vtable at `0x1FE310` (symbol `_ZTVN7DinoCap9menuSceneE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xC9038` (`sub_C9038`) | indexed |
| 1 | +0xC / +0x4 | `0xC8F98` (`sub_C8F98`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xC91D0` (`sub_C91D0`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-0C91D0.md) |
| 5 | +0x1C / +0x14 | `0xCA83C` (`sub_CA83C`) | indexed |
| 6 | +0x20 / +0x18 | `0xD05AC` (`sub_D05AC`) | [dossier](../functions/dossiers/engine-platform/FUN-0D05AC.md) |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0xCAA38` (`sub_CAA38`) | indexed |
| 9 | +0x2C / +0x24 | `0x189E88` (`nullsub_47`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0xCF584` (`sub_CF584`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'DinoCap::pauseCtrl`

Vtable at `0x1FE370` (symbol `_ZTVN7DinoCap9pauseCtrlE`), 32 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD0C50` (`sub_D0C50`) | indexed |
| 1 | +0xC / +0x4 | `0xD0BB0` (`sub_D0BB0`) | indexed |
| 2 | +0x10 / +0x8 | `0xD1444` (`sub_D1444`) | indexed |
| 3 | +0x14 / +0xC | `0x183D9C` (`sub_183D9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0xD1CC8` (`sub_D1CC8`) | indexed |
| 26 | +0x70 / +0x68 | `0xD2214` (`sub_D2214`) | indexed |
| 27 | +0x74 / +0x6C | `0xD1CF8` (`sub_D1CF8`) | indexed |
| 28 | +0x78 / +0x70 | `0xD0DAC` (`sub_D0DAC`) | indexed |
| 29 | +0x7C / +0x74 | `0xD0DCC` (`sub_D0DCC`) | indexed |
| 30 | +0x80 / +0x78 | `0xD0E00` (`sub_D0E00`) | indexed |
| 31 | +0x84 / +0x7C | `0xD0DEC` (`sub_D0DEC`) | indexed |

## `vtable for'DinoCap::playerClass`

Vtable at `0x1FE450` (symbol `_ZTVN7DinoCap11playerClassE`), 13 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD24A0` (`sub_D24A0`) | indexed |
| 1 | +0xC / +0x4 | `0xD2400` (`sub_D2400`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0xD2614` (`sub_D2614`) | indexed |
| 5 | +0x1C / +0x14 | `0xD2900` (`nullsub_9`) | indexed |
| 6 | +0x20 / +0x18 | `0xD2904` (`sub_D2904`) | [dossier](../functions/dossiers/animation-rendering/FUN-0D2904.md) |
| 7 | +0x24 / +0x1C | `0xD2D9C` (`sub_D2D9C`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |
| 9 | +0x2C / +0x24 | `0xD31DC` (`sub_D31DC`) | indexed |
| 10 | +0x30 / +0x28 | `0xD302C` (`sub_D302C`) | indexed |
| 11 | +0x34 / +0x2C | `0xD3900` (`sub_D3900`) | indexed |
| 12 | +0x38 / +0x30 | `0xD3974` (`sub_D3974`) | indexed |

## `vtable for'DinoCap::testApp`

Vtable at `0x1FE4A0` (symbol `_ZTVN7DinoCap7testAppE`), 8 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD5B08` (`sub_D5B08`) | indexed |
| 1 | +0xC / +0x4 | `0xD5A68` (`sub_D5A68`) | indexed |
| 2 | +0x10 / +0x8 | `0x188B80` (`sub_188B80`) | indexed |
| 3 | +0x14 / +0xC | `0x188B84` (`sub_188B84`) | indexed |
| 4 | +0x18 / +0x10 | `0xD5BC0` (`sub_D5BC0`) | [dossier](../functions/dossiers/engine-platform/FUN-0D5BC0.md) |
| 5 | +0x1C / +0x14 | `0xD61F0` (`sub_D61F0`) | indexed |
| 6 | +0x20 / +0x18 | `0x188B94` (`nullsub_35`) | indexed |
| 7 | +0x24 / +0x1C | `0x188B98` (`nullsub_36`) | indexed |

## `vtable for'DinoCap::TiXmlNode`

Vtable at `0x1FE4E0` (symbol `_ZTVN7DinoCap9TiXmlNodeE`), 2 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD6768` (`sub_D6768`) | indexed |
| 1 | +0xC / +0x4 | `0xD66C8` (`sub_D66C8`) | indexed |

## `vtable for'DinoCap::TiXmlElement`

Vtable at `0x1FE530` (symbol `_ZTVN7DinoCap12TiXmlElementE`), 18 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD6CC4` (`sub_D6CC4`) | indexed |
| 1 | +0xC / +0x4 | `0xD6C24` (`sub_D6C24`) | indexed |
| 2 | +0x10 / +0x8 | `0xD705C` (`sub_D705C`) | indexed |
| 3 | +0x14 / +0xC | `0xD93A8` (`sub_D93A8`) | indexed |
| 4 | +0x18 / +0x10 | `0xD7FF8` (`__ZNK7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xD82BC` (`__ZNK7DinoCap12TiXmlElement9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xC0C78` (`__ZNK7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xC0C7C` (`__ZNK7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xC0C80` (`__ZNK7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xC0C84` (`__ZNK7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xD7FFC` (`__ZN7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xD82C0` (`__ZN7DinoCap12TiXmlElement9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xC0C90` (`__ZN7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xC0C94` (`__ZN7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xC0C98` (`__ZN7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xC0C9C` (`__ZN7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD7214` (`sub_D7214`) | indexed |
| 17 | +0x4C / +0x44 | `0xD71CC` (`sub_D71CC`) | indexed |

## `vtable for'DinoCap::TiXmlComment`

Vtable at `0x1FE580` (symbol `_ZTVN7DinoCap12TiXmlCommentE`), 18 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD8000` (`__ZN7DinoCap12TiXmlCommentD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xD8004` (`__ZN7DinoCap12TiXmlCommentD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD7808` (`sub_D7808`) | indexed |
| 3 | +0x14 / +0xC | `0xD9A80` (`sub_D9A80`) | indexed |
| 4 | +0x18 / +0x10 | `0xD7FF8` (`__ZNK7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xC0C74` (`__ZNK7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xD80A4` (`__ZNK7DinoCap12TiXmlComment9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xC0C7C` (`__ZNK7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xC0C80` (`__ZNK7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xC0C84` (`__ZNK7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xD7FFC` (`__ZN7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xC0C8C` (`__ZN7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xD80A8` (`__ZN7DinoCap12TiXmlComment9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xC0C94` (`__ZN7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xC0C98` (`__ZN7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xC0C9C` (`__ZN7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD7890` (`sub_D7890`) | indexed |
| 17 | +0x4C / +0x44 | `0xD787C` (`sub_D787C`) | indexed |

## `vtable for'DinoCap::TiXmlDeclaration`

Vtable at `0x1FE5D0` (symbol `_ZTVN7DinoCap16TiXmlDeclarationE`), 19 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD8158` (`__ZN7DinoCap16TiXmlDeclarationD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xD815C` (`__ZN7DinoCap16TiXmlDeclarationD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD81FC` (`__ZNK7DinoCap16TiXmlDeclaration5PrintEP7__sFILEi`) | indexed |
| 3 | +0x14 / +0xC | `0xD9ED8` (`sub_D9ED8`) | indexed |
| 4 | +0x18 / +0x10 | `0xD7FF8` (`__ZNK7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xC0C74` (`__ZNK7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xC0C78` (`__ZNK7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xC0C7C` (`__ZNK7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xC0C80` (`__ZNK7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xD8208` (`__ZNK7DinoCap16TiXmlDeclaration13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xD7FFC` (`__ZN7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xC0C8C` (`__ZN7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xC0C90` (`__ZN7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xC0C94` (`__ZN7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xC0C98` (`__ZN7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xD820C` (`__ZN7DinoCap16TiXmlDeclaration13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD7D08` (`sub_D7D08`) | indexed |
| 17 | +0x4C / +0x44 | `0xD7CF4` (`sub_D7CF4`) | indexed |
| 18 | +0x50 / +0x48 | `0xD7BA4` (`sub_D7BA4`) | indexed |

## `vtable for'DinoCap::TiXmlText`

Vtable at `0x1FE640` (symbol `_ZTVN7DinoCap9TiXmlTextE`), 18 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD80AC` (`__ZN7DinoCap9TiXmlTextD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xD80B0` (`__ZN7DinoCap9TiXmlTextD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD7908` (`sub_D7908`) | indexed |
| 3 | +0x14 / +0xC | `0xD9CF0` (`sub_D9CF0`) | indexed |
| 4 | +0x18 / +0x10 | `0xD7FF8` (`__ZNK7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xC0C74` (`__ZNK7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xC0C78` (`__ZNK7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xC0C7C` (`__ZNK7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xD8150` (`__ZNK7DinoCap9TiXmlText6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xC0C84` (`__ZNK7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xD7FFC` (`__ZN7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xC0C8C` (`__ZN7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xC0C90` (`__ZN7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xC0C94` (`__ZN7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xD8154` (`__ZN7DinoCap9TiXmlText6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xC0C9C` (`__ZN7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD7A68` (`sub_D7A68`) | indexed |
| 17 | +0x4C / +0x44 | `0xD7A54` (`sub_D7A54`) | indexed |

## `vtable for'DinoCap::TiXmlUnknown`

Vtable at `0x1FE6B0` (symbol `_ZTVN7DinoCap12TiXmlUnknownE`), 18 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD8210` (`__ZN7DinoCap12TiXmlUnknownD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xD8214` (`__ZN7DinoCap12TiXmlUnknownD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD7D68` (`sub_D7D68`) | indexed |
| 3 | +0x14 / +0xC | `0xD99E0` (`sub_D99E0`) | indexed |
| 4 | +0x18 / +0x10 | `0xD7FF8` (`__ZNK7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xC0C74` (`__ZNK7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xC0C78` (`__ZNK7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xD82B4` (`__ZNK7DinoCap12TiXmlUnknown9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xC0C80` (`__ZNK7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xC0C84` (`__ZNK7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xD7FFC` (`__ZN7DinoCap9TiXmlNode10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xC0C8C` (`__ZN7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xC0C90` (`__ZN7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xD82B8` (`__ZN7DinoCap12TiXmlUnknown9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xC0C98` (`__ZN7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xC0C9C` (`__ZN7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD7DC8` (`sub_D7DC8`) | indexed |
| 17 | +0x4C / +0x44 | `0xD7DB4` (`sub_D7DB4`) | indexed |

## `vtable for'DinoCap::TiXmlAttribute`

Vtable at `0x1FE730` (symbol `_ZTVN7DinoCap14TiXmlAttributeE`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xD7FB4` (`__ZN7DinoCap14TiXmlAttributeD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xD8318` (`__ZN7DinoCap14TiXmlAttributeD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD8360` (`__ZNK7DinoCap14TiXmlAttribute5PrintEP7__sFILEi`) | indexed |
| 3 | +0x14 / +0xC | `0xD9B50` (`sub_D9B50`) | indexed |

## `vtable for'DinoCap::TiXmlDocument`

Vtable at `0x1FE760` (symbol `_ZTVN7DinoCap13TiXmlDocumentE`), 18 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xC0AC8` (`__ZN7DinoCap13TiXmlDocumentD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0xC0BD0` (`__ZN7DinoCap13TiXmlDocumentD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0xD7588` (`sub_D7588`) | indexed |
| 3 | +0x14 / +0xC | `0xD8EA0` (`sub_D8EA0`) | indexed |
| 4 | +0x18 / +0x10 | `0xC0C70` (`__ZNK7DinoCap13TiXmlDocument10ToDocumentEv`) | indexed |
| 5 | +0x1C / +0x14 | `0xC0C74` (`__ZNK7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 6 | +0x20 / +0x18 | `0xC0C78` (`__ZNK7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 7 | +0x24 / +0x1C | `0xC0C7C` (`__ZNK7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 8 | +0x28 / +0x20 | `0xC0C80` (`__ZNK7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 9 | +0x2C / +0x24 | `0xC0C84` (`__ZNK7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 10 | +0x30 / +0x28 | `0xC0C88` (`__ZN7DinoCap13TiXmlDocument10ToDocumentEv`) | indexed |
| 11 | +0x34 / +0x2C | `0xC0C8C` (`__ZN7DinoCap9TiXmlNode9ToElementEv`) | indexed |
| 12 | +0x38 / +0x30 | `0xC0C90` (`__ZN7DinoCap9TiXmlNode9ToCommentEv`) | indexed |
| 13 | +0x3C / +0x34 | `0xC0C94` (`__ZN7DinoCap9TiXmlNode9ToUnknownEv`) | indexed |
| 14 | +0x40 / +0x38 | `0xC0C98` (`__ZN7DinoCap9TiXmlNode6ToTextEv`) | indexed |
| 15 | +0x44 / +0x3C | `0xC0C9C` (`__ZN7DinoCap9TiXmlNode13ToDeclarationEv`) | indexed |
| 16 | +0x48 / +0x40 | `0xD74CC` (`sub_D74CC`) | indexed |
| 17 | +0x4C / +0x44 | `0xD75D8` (`sub_D75D8`) | indexed |

## `vtable for'DinoCap::UIMoveCtrl`

Vtable at `0x1FE7C0` (symbol `_ZTVN7DinoCap10UIMoveCtrlE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xDA3BC` (`j___ZN7DinoCap9houseCtrlD1Ev_0`) | indexed |
| 1 | +0xC / +0x4 | `0xDA31C` (`sub_DA31C`) | indexed |
| 2 | +0x10 / +0x8 | `0x183D3C` (`sub_183D3C`) | indexed |
| 3 | +0x14 / +0xC | `0x183D9C` (`sub_183D9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x183DE4` (`sub_183DE4`) | indexed |
| 26 | +0x70 / +0x68 | `0xDA3C0` (`sub_DA3C0`) | indexed |
| 27 | +0x74 / +0x6C | `0xDA6D8` (`sub_DA6D8`) | indexed |

## `vtable for'DinoCap::UILevelCtrl`

Vtable at `0x1FE840` (symbol `_ZTVN7DinoCap11UILevelCtrlE`), 32 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xDAAB0` (`sub_DAAB0`) | indexed |
| 1 | +0xC / +0x4 | `0xDAA10` (`sub_DAA10`) | indexed |
| 2 | +0x10 / +0x8 | `0xDAC24` (`sub_DAC24`) | indexed |
| 3 | +0x14 / +0xC | `0xDB4EC` (`sub_DB4EC`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0xDB600` (`sub_DB600`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x183414` (`sub_183414`) | indexed |
| 22 | +0x60 / +0x58 | `0x183428` (`sub_183428`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0xDAEF0` (`sub_DAEF0`) | indexed |
| 26 | +0x70 / +0x68 | `0xDAF34` (`sub_DAF34`) | indexed |
| 27 | +0x74 / +0x6C | `0xDB0CC` (`sub_DB0CC`) | indexed |
| 28 | +0x78 / +0x70 | `0xDAE80` (`sub_DAE80`) | indexed |
| 29 | +0x7C / +0x74 | `0xDAEA0` (`sub_DAEA0`) | indexed |
| 30 | +0x80 / +0x78 | `0xDAEC0` (`sub_DAEC0`) | indexed |
| 31 | +0x84 / +0x7C | `0xDAEDC` (`sub_DAEDC`) | indexed |

## `vtable for'DinoCap::upgradeScene`

Vtable at `0x1FE930` (symbol `_ZTVN7DinoCap12upgradeSceneE`), 17 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xDBB1C` (`sub_DBB1C`) | indexed |
| 1 | +0xC / +0x4 | `0xDBA7C` (`sub_DBA7C`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xDBC40` (`sub_DBC40`) | [dossier](../functions/dossiers/saves-shop-progression/FUN-0DBC40.md) |
| 5 | +0x1C / +0x14 | `0xE3270` (`sub_E3270`) | indexed |
| 6 | +0x20 / +0x18 | `0xE4570` (`sub_E4570`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0xE450C` (`sub_E450C`) | indexed |
| 9 | +0x2C / +0x24 | `0xE49E8` (`sub_E49E8`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0xC2A64` (`sub_C2A64`) | indexed |
| 12 | +0x38 / +0x30 | `0xC68D4` (`sub_C68D4`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |
| 14 | +0x40 / +0x38 | `0xE869C` (`sub_E869C`) | indexed |
| 15 | +0x44 / +0x3C | `0xC7FE8` (`sub_C7FE8`) | indexed |
| 16 | +0x48 / +0x40 | `0xC8798` (`sub_C8798`) | indexed |

## `vtable for'DinoCap::PreTitle`

Vtable at `0x1FE990` (symbol `_ZTVN7DinoCap8PreTitleE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xEB41C` (`sub_EB41C`) | indexed |
| 1 | +0xC / +0x4 | `0xEB37C` (`sub_EB37C`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xEB4D4` (`sub_EB4D4`) | indexed |
| 5 | +0x1C / +0x14 | `0xEB810` (`sub_EB810`) | indexed |
| 6 | +0x20 / +0x18 | `0xEB814` (`sub_EB814`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x189E84` (`nullsub_46`) | indexed |
| 9 | +0x2C / +0x24 | `0x189E88` (`nullsub_47`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x189E94` (`nullsub_50`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'DinoCap::AdScene`

Vtable at `0x1FE9E0` (symbol `_ZTVN7DinoCap7AdSceneE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xEBBCC` (`sub_EBBCC`) | indexed |
| 1 | +0xC / +0x4 | `0xEBB2C` (`sub_EBB2C`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xEBC84` (`sub_EBC84`) | indexed |
| 5 | +0x1C / +0x14 | `0xEC078` (`sub_EC078`) | indexed |
| 6 | +0x20 / +0x18 | `0xEC260` (`sub_EC260`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x189E84` (`nullsub_46`) | indexed |
| 9 | +0x2C / +0x24 | `0xEC030` (`sub_EC030`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x189E94` (`nullsub_50`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'DinoCap::AdSceneiPad`

Vtable at `0x1FEA30` (symbol `_ZTVN7DinoCap11AdSceneiPadE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xEC36C` (`sub_EC36C`) | indexed |
| 1 | +0xC / +0x4 | `0xEC2CC` (`sub_EC2CC`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xEC424` (`sub_EC424`) | indexed |
| 5 | +0x1C / +0x14 | `0xEC814` (`sub_EC814`) | indexed |
| 6 | +0x20 / +0x18 | `0xEC9FC` (`sub_EC9FC`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x189E84` (`nullsub_46`) | indexed |
| 9 | +0x2C / +0x24 | `0xEC7C8` (`sub_EC7C8`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x189E94` (`nullsub_50`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'DinoCap::UIMaskCtrl`

Vtable at `0x1FEA80` (symbol `_ZTVN7DinoCap10UIMaskCtrlE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xECB24` (`j_nullsub_30`) | indexed |
| 1 | +0xC / +0x4 | `0xECA84` (`sub_ECA84`) | indexed |
| 2 | +0x10 / +0x8 | `0x183238` (`sub_183238`) | indexed |
| 3 | +0x14 / +0xC | `0x18324C` (`sub_18324C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x183414` (`sub_183414`) | indexed |
| 22 | +0x60 / +0x58 | `0x183428` (`sub_183428`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x1834FC` (`nullsub_31`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0xECB28` (`sub_ECB28`) | indexed |

## `vtable for'DinoCap::PreTitleiPad`

Vtable at `0x1FEB10` (symbol `_ZTVN7DinoCap12PreTitleiPadE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0xECBCC` (`sub_ECBCC`) | indexed |
| 1 | +0xC / +0x4 | `0xECB2C` (`sub_ECB2C`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0xECC84` (`sub_ECC84`) | indexed |
| 5 | +0x1C / +0x14 | `0xED28C` (`sub_ED28C`) | indexed |
| 6 | +0x20 / +0x18 | `0xED474` (`sub_ED474`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x189E84` (`nullsub_46`) | indexed |
| 9 | +0x2C / +0x24 | `0x189E88` (`nullsub_47`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x189E94` (`nullsub_50`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'b2ContactManager`

Vtable at `0x200900` (symbol `_ZTV16b2ContactManager`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13AD00` (`__ZN16b2ContactManagerD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0x13ADC8` (`__ZN16b2ContactManagerD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x138380` (`sub_138380`) | indexed |
| 3 | +0x14 / +0xC | `0x138458` (`sub_138458`) | indexed |

## `vtable for'b2ContactFilter`

Vtable at `0x200930` (symbol `_ZTV15b2ContactFilter`), 3 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13AE38` (`__ZN15b2ContactFilterD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0x13AE3C` (`__ZN15b2ContactFilterD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x13ADD8` (`sub_13ADD8`) | indexed |

## `vtable for'b2CircleContact`

Vtable at `0x200950` (symbol `_ZTV15b2CircleContact`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13B3B8` (`__ZN15b2CircleContact12GetManifoldsEv`) | indexed |
| 1 | +0xC / +0x4 | `0x13B3BC` (`__ZN15b2CircleContactD1Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x13B3C0` (`__ZN15b2CircleContactD0Ev`) | indexed |
| 3 | +0x14 / +0xC | `0x13AF14` (`sub_13AF14`) | indexed |

## `vtable for'b2PolyAndCircleContact`

Vtable at `0x200980` (symbol `_ZTV22b2PolyAndCircleContact`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13CAE0` (`__ZN22b2PolyAndCircleContact12GetManifoldsEv`) | indexed |
| 1 | +0xC / +0x4 | `0x13CAE4` (`__ZN22b2PolyAndCircleContactD1Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x13CAE8` (`__ZN22b2PolyAndCircleContactD0Ev`) | indexed |
| 3 | +0x14 / +0xC | `0x13C58C` (`sub_13C58C`) | indexed |

## `vtable for'b2PolygonContact`

Vtable at `0x2009B0` (symbol `_ZTV16b2PolygonContact`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x13D104` (`__ZN16b2PolygonContact12GetManifoldsEv`) | indexed |
| 1 | +0xC / +0x4 | `0x13D108` (`__ZN16b2PolygonContactD1Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x13D10C` (`__ZN16b2PolygonContactD0Ev`) | indexed |
| 3 | +0x14 / +0xC | `0x13CBB0` (`sub_13CBB0`) | indexed |

## `vtable for'CPVRTString`

Vtable at `0x20160C` (symbol `_ZTV11CPVRTString`), 2 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x16390C` (`sub_16390C`) | indexed |
| 1 | +0xC / +0x4 | `0x1638E8` (`sub_1638E8`) | indexed |

## `vtable for'Triniti2D::EngineShell`

Vtable at `0x201630` (symbol `_ZTVN9Triniti2D11EngineShellE`), 12 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x16A1B4` (`sub_16A1B4`) | indexed |
| 1 | +0xC / +0x4 | `0x16A108` (`sub_16A108`) | indexed |
| 2 | +0x10 / +0x8 | `0x16A1D0` (`sub_16A1D0`) | indexed |
| 3 | +0x14 / +0xC | `0x16A250` (`sub_16A250`) | indexed |
| 4 | +0x18 / +0x10 | `0x16A1FC` (`sub_16A1FC`) | indexed |
| 5 | +0x1C / +0x14 | `0x16A240` (`sub_16A240`) | indexed |
| 6 | +0x20 / +0x18 | `0x16A274` (`sub_16A274`) | indexed |
| 7 | +0x24 / +0x1C | `0x16A330` (`sub_16A330`) | indexed |
| 8 | +0x28 / +0x20 | `0x16A338` (`sub_16A338`) | indexed |
| 9 | +0x2C / +0x24 | `0x16A340` (`sub_16A340`) | indexed |
| 10 | +0x30 / +0x28 | `0x16A348` (`sub_16A348`) | indexed |
| 11 | +0x34 / +0x2C | `0x16A350` (`sub_16A350`) | indexed |

## `vtable for'Triniti2D::GameObject`

Vtable at `0x201680` (symbol `_ZTVN9Triniti2D10GameObjectE`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x171F2C` (`sub_171F2C`) | indexed |
| 1 | +0xC / +0x4 | `0x171E8C` (`sub_171E8C`) | indexed |
| 2 | +0x10 / +0x8 | `0x172700` (`sub_172700`) | indexed |
| 3 | +0x14 / +0xC | `0x172708` (`sub_172708`) | indexed |

## `vtable for'Triniti2D::Scene`

Vtable at `0x2016B0` (symbol `_ZTVN9Triniti2D5SceneE`), 4 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x17B3D0` (`sub_17B3D0`) | indexed |
| 1 | +0xC / +0x4 | `0x17B330` (`sub_17B330`) | indexed |
| 2 | +0x10 / +0x8 | `0x17C000` (`sub_17C000`) | indexed |
| 3 | +0x14 / +0xC | `0x17C008` (`sub_17C008`) | indexed |

## `vtable for'Triniti2D::Sprite`

Vtable at `0x2016E0` (symbol `_ZTVN9Triniti2D6SpriteE`), 3 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1748E8` (`__ZN9Triniti2D6SpriteD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0x1748EC` (`__ZN9Triniti2D6SpriteD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x17C9C8` (`sub_17C9C8`) | indexed |

## `vtable for'Triniti2D::UIContainer`

Vtable at `0x201700` (symbol `_ZTVN9Triniti2D11UIContainerE`), 3 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x183170` (`nullsub_27`) | indexed |
| 1 | +0xC / +0x4 | `0x18316C` (`j___ZN16b2ContactManagerD0Ev_0`) | indexed |
| 2 | +0x10 / +0x8 | `0x183178` (`__ZN9Triniti2D11UIContainer10GetTextureERKSsRNS_7TextureE`) | indexed |

## `vtable for'Triniti2D::UIControl`

Vtable at `0x201740` (symbol `_ZTVN9Triniti2D9UIControlE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x183230` (`nullsub_29`) | indexed |
| 1 | +0xC / +0x4 | `0x18322C` (`j___ZN16b2ContactManagerD0Ev_0_0`) | indexed |
| 2 | +0x10 / +0x8 | `0x183238` (`sub_183238`) | indexed |
| 3 | +0x14 / +0xC | `0x18324C` (`sub_18324C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x183414` (`sub_183414`) | indexed |
| 22 | +0x60 / +0x58 | `0x183428` (`sub_183428`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x1834FC` (`nullsub_31`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0x183504` (`sub_183504`) | indexed |

## `vtable for'Triniti2D::UIControlVisible`

Vtable at `0x2017C0` (symbol `_ZTVN9Triniti2D16UIControlVisibleE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1835D0` (`sub_1835D0`) | indexed |
| 1 | +0xC / +0x4 | `0x183530` (`sub_183530`) | indexed |
| 2 | +0x10 / +0x8 | `0x183238` (`sub_183238`) | indexed |
| 3 | +0x14 / +0xC | `0x18324C` (`sub_18324C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x1834FC` (`nullsub_31`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0x183504` (`sub_183504`) | indexed |

## `vtable for'Triniti2D::UIClickButton`

Vtable at `0x201840` (symbol `_ZTVN9Triniti2D13UIClickButtonE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x183ADC` (`j___ZN7DinoCap9houseCtrlD1Ev_0_0`) | indexed |
| 1 | +0xC / +0x4 | `0x183A3C` (`sub_183A3C`) | indexed |
| 2 | +0x10 / +0x8 | `0x1838B4` (`sub_1838B4`) | indexed |
| 3 | +0x14 / +0xC | `0x183AE0` (`sub_183AE0`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183914` (`sub_183914`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x1839E0` (`sub_1839E0`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0x183B8C` (`sub_183B8C`) | indexed |

## `vtable for'Triniti2D::UIImage`

Vtable at `0x2018C0` (symbol `_ZTVN9Triniti2D7UIImageE`), 28 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x183D34` (`j___ZN7DinoCap9houseCtrlD1Ev_0_0_0`) | indexed |
| 1 | +0xC / +0x4 | `0x183C94` (`sub_183C94`) | indexed |
| 2 | +0x10 / +0x8 | `0x183D3C` (`sub_183D3C`) | indexed |
| 3 | +0x14 / +0xC | `0x183D9C` (`sub_183D9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x183DE4` (`sub_183DE4`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0x183DF0` (`sub_183DF0`) | indexed |

## `vtable for'Triniti2D::UILabel`

Vtable at `0x201980` (symbol `_ZTVN9Triniti2D7UILabelE`), 29 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1848AC` (`sub_1848AC`) | indexed |
| 1 | +0xC / +0x4 | `0x18480C` (`sub_18480C`) | indexed |
| 2 | +0x10 / +0x8 | `0x183238` (`sub_183238`) | indexed |
| 3 | +0x14 / +0xC | `0x184B34` (`sub_184B34`) | indexed |
| 4 | +0x18 / +0x10 | `0x183258` (`sub_183258`) | indexed |
| 5 | +0x1C / +0x14 | `0x183264` (`sub_183264`) | indexed |
| 6 | +0x20 / +0x18 | `0x183290` (`sub_183290`) | indexed |
| 7 | +0x24 / +0x1C | `0x1832AC` (`sub_1832AC`) | indexed |
| 8 | +0x28 / +0x20 | `0x1832D8` (`sub_1832D8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1832F4` (`sub_1832F4`) | indexed |
| 10 | +0x30 / +0x28 | `0x18331C` (`sub_18331C`) | indexed |
| 11 | +0x34 / +0x2C | `0x183334` (`sub_183334`) | indexed |
| 12 | +0x38 / +0x30 | `0x18335C` (`sub_18335C`) | indexed |
| 13 | +0x3C / +0x34 | `0x183374` (`sub_183374`) | indexed |
| 14 | +0x40 / +0x38 | `0x18339C` (`sub_18339C`) | indexed |
| 15 | +0x44 / +0x3C | `0x1833B4` (`sub_1833B4`) | indexed |
| 16 | +0x48 / +0x40 | `0x1833DC` (`sub_1833DC`) | indexed |
| 17 | +0x4C / +0x44 | `0x1833F4` (`sub_1833F4`) | indexed |
| 18 | +0x50 / +0x48 | `0x1833FC` (`sub_1833FC`) | indexed |
| 19 | +0x54 / +0x4C | `0x183404` (`sub_183404`) | indexed |
| 20 | +0x58 / +0x50 | `0x18340C` (`sub_18340C`) | indexed |
| 21 | +0x5C / +0x54 | `0x1836E8` (`sub_1836E8`) | indexed |
| 22 | +0x60 / +0x58 | `0x183730` (`sub_183730`) | indexed |
| 23 | +0x64 / +0x5C | `0x183430` (`sub_183430`) | indexed |
| 24 | +0x68 / +0x60 | `0x1834DC` (`sub_1834DC`) | indexed |
| 25 | +0x6C / +0x64 | `0x184B48` (`sub_184B48`) | indexed |
| 26 | +0x70 / +0x68 | `0x183500` (`nullsub_32`) | indexed |
| 27 | +0x74 / +0x6C | `0x184B8C` (`sub_184B8C`) | indexed |
| 28 | +0x78 / +0x70 | `0x184BA0` (`sub_184BA0`) | indexed |

## `vtable for'Triniti2D::UIManager`

Vtable at `0x201A10` (symbol `_ZTVN9Triniti2D9UIManagerE`), 10 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x185988` (`sub_185988`) | indexed |
| 1 | +0xC / +0x4 | `0x1858E8` (`sub_1858E8`) | indexed |
| 2 | +0x10 / +0x8 | `0x185E58` (`sub_185E58`) | indexed |
| 3 | +0x14 / +0xC | `0x185E9C` (`sub_185E9C`) | indexed |
| 4 | +0x18 / +0x10 | `0x185EE0` (`sub_185EE0`) | indexed |
| 5 | +0x1C / +0x14 | `0x185F24` (`sub_185F24`) | indexed |
| 6 | +0x20 / +0x18 | `0x185F3C` (`sub_185F3C`) | indexed |
| 7 | +0x24 / +0x1C | `0x185F54` (`sub_185F54`) | indexed |
| 8 | +0x28 / +0x20 | `0x185F5C` (`sub_185F5C`) | indexed |
| 9 | +0x2C / +0x24 | `0x185F64` (`sub_185F64`) | indexed |

## `vtable for'Triniti2D::CppGameCallBack`

Vtable at `0x201A50` (symbol `_ZTVN9Triniti2D15CppGameCallBackE`), 8 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x188B00` (`sub_188B00`) | indexed |
| 1 | +0xC / +0x4 | `0x188A28` (`sub_188A28`) | indexed |
| 2 | +0x10 / +0x8 | `0x188B80` (`sub_188B80`) | indexed |
| 3 | +0x14 / +0xC | `0x188B84` (`sub_188B84`) | indexed |
| 4 | +0x18 / +0x10 | `0x188B8C` (`nullsub_33`) | indexed |
| 5 | +0x1C / +0x14 | `0x188B90` (`nullsub_34`) | indexed |
| 6 | +0x20 / +0x18 | `0x188B94` (`nullsub_35`) | indexed |
| 7 | +0x24 / +0x1C | `0x188B98` (`nullsub_36`) | indexed |

## `vtable for'Triniti2D::CppGameObjectCallBack`

Vtable at `0x201A90` (symbol `_ZTVN9Triniti2D21CppGameObjectCallBackE`), 9 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x188CA4` (`sub_188CA4`) | indexed |
| 1 | +0xC / +0x4 | `0x188BCC` (`sub_188BCC`) | indexed |
| 2 | +0x10 / +0x8 | `0x188D24` (`sub_188D24`) | indexed |
| 3 | +0x14 / +0xC | `0x188D28` (`sub_188D28`) | indexed |
| 4 | +0x18 / +0x10 | `0x188D30` (`nullsub_37`) | indexed |
| 5 | +0x1C / +0x14 | `0x188D34` (`nullsub_38`) | indexed |
| 6 | +0x20 / +0x18 | `0x188D38` (`nullsub_39`) | indexed |
| 7 | +0x24 / +0x1C | `0x188D3C` (`nullsub_40`) | indexed |
| 8 | +0x28 / +0x20 | `0x188D40` (`nullsub_41`) | indexed |

## `vtable for'Triniti2D::CppLogicManager`

Vtable at `0x201AD0` (symbol `_ZTVN9Triniti2D15CppLogicManagerE`), 12 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x188E20` (`sub_188E20`) | indexed |
| 1 | +0xC / +0x4 | `0x188D80` (`sub_188D80`) | indexed |
| 2 | +0x10 / +0x8 | `0x188FDC` (`sub_188FDC`) | indexed |
| 3 | +0x14 / +0xC | `0x188FE0` (`sub_188FE0`) | indexed |
| 4 | +0x18 / +0x10 | `0x188FE8` (`sub_188FE8`) | indexed |
| 5 | +0x1C / +0x14 | `0x18907C` (`sub_18907C`) | indexed |
| 6 | +0x20 / +0x18 | `0x1890F0` (`sub_1890F0`) | indexed |
| 7 | +0x24 / +0x1C | `0x1891B8` (`sub_1891B8`) | indexed |
| 8 | +0x28 / +0x20 | `0x1891C8` (`sub_1891C8`) | indexed |
| 9 | +0x2C / +0x24 | `0x1892A8` (`sub_1892A8`) | indexed |
| 10 | +0x30 / +0x28 | `0x1892B8` (`sub_1892B8`) | indexed |
| 11 | +0x34 / +0x2C | `0x1893A0` (`sub_1893A0`) | indexed |

## `vtable for'Triniti2D::CppSceneCallBack`

Vtable at `0x201B20` (symbol `_ZTVN9Triniti2D16CppSceneCallBackE`), 14 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x189DE8` (`sub_189DE8`) | indexed |
| 1 | +0xC / +0x4 | `0x189D10` (`sub_189D10`) | indexed |
| 2 | +0x10 / +0x8 | `0x189E68` (`sub_189E68`) | indexed |
| 3 | +0x14 / +0xC | `0x189E6C` (`sub_189E6C`) | indexed |
| 4 | +0x18 / +0x10 | `0x189E74` (`nullsub_42`) | indexed |
| 5 | +0x1C / +0x14 | `0x189E78` (`nullsub_43`) | indexed |
| 6 | +0x20 / +0x18 | `0x189E7C` (`nullsub_44`) | indexed |
| 7 | +0x24 / +0x1C | `0x189E80` (`nullsub_45`) | indexed |
| 8 | +0x28 / +0x20 | `0x189E84` (`nullsub_46`) | indexed |
| 9 | +0x2C / +0x24 | `0x189E88` (`nullsub_47`) | indexed |
| 10 | +0x30 / +0x28 | `0x189E8C` (`nullsub_48`) | indexed |
| 11 | +0x34 / +0x2C | `0x189E90` (`nullsub_49`) | indexed |
| 12 | +0x38 / +0x30 | `0x189E94` (`nullsub_50`) | indexed |
| 13 | +0x3C / +0x34 | `0x189E98` (`nullsub_51`) | indexed |

## `vtable for'Triniti2D::UISprite`

Vtable at `0x201D00` (symbol `_ZTVN9Triniti2D8UISpriteE`), 3 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x183E60` (`__ZN9Triniti2D8UISpriteD1Ev`) | indexed |
| 1 | +0xC / +0x4 | `0x184234` (`__ZN9Triniti2D8UISpriteD0Ev`) | indexed |
| 2 | +0x10 / +0x8 | `0x19A524` (`sub_19A524`) | indexed |

## `vtable for'TrinitiAnalyticsBufferStreamWriter`

Vtable at `0x201D60` (symbol `_ZTV34TrinitiAnalyticsBufferStreamWriter`), 6 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x19DA44` (`nullsub_52`) | indexed |
| 1 | +0xC / +0x4 | `0x19DA40` (`j___ZN16b2ContactManagerD0Ev_0_1`) | indexed |
| 2 | +0x10 / +0x8 | `0x19DA48` (`sub_19DA48`) | indexed |
| 3 | +0x14 / +0xC | `0x19DA68` (`sub_19DA68`) | indexed |
| 4 | +0x18 / +0x10 | `0x19DA88` (`sub_19DA88`) | indexed |
| 5 | +0x1C / +0x14 | `0x19DAA4` (`sub_19DAA4`) | indexed |

## `vtable for'TrinitiAnalyticsJsonString`

Vtable at `0x201DA0` (symbol `_ZTV26TrinitiAnalyticsJsonString`), 11 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1A0354` (`sub_1A0354`) | indexed |
| 1 | +0xC / +0x4 | `0x1A032C` (`sub_1A032C`) | indexed |
| 2 | +0x10 / +0x8 | `0x1A0378` (`sub_1A0378`) | indexed |
| 3 | +0x14 / +0xC | `0x1A0384` (`sub_1A0384`) | indexed |
| 4 | +0x18 / +0x10 | `0x1A0130` (`sub_1A0130`) | indexed |
| 5 | +0x1C / +0x14 | `0x1A0134` (`sub_1A0134`) | indexed |
| 6 | +0x20 / +0x18 | `0x1A0138` (`sub_1A0138`) | indexed |
| 7 | +0x24 / +0x1C | `0x1A013C` (`sub_1A013C`) | indexed |
| 8 | +0x28 / +0x20 | `0x1A0140` (`sub_1A0140`) | indexed |
| 9 | +0x2C / +0x24 | `0x1A0144` (`sub_1A0144`) | indexed |
| 10 | +0x30 / +0x28 | `0x1A038C` (`sub_1A038C`) | indexed |

## `vtable for'TrinitiAnalyticsJsonNumber`

Vtable at `0x201DE0` (symbol `_ZTV26TrinitiAnalyticsJsonNumber`), 11 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1A06F4` (`nullsub_53`) | indexed |
| 1 | +0xC / +0x4 | `0x1A06F0` (`j___ZN16b2ContactManagerD0Ev_0_2`) | indexed |
| 2 | +0x10 / +0x8 | `0x1A06F8` (`sub_1A06F8`) | indexed |
| 3 | +0x14 / +0xC | `0x1A012C` (`sub_1A012C`) | indexed |
| 4 | +0x18 / +0x10 | `0x1A0704` (`sub_1A0704`) | indexed |
| 5 | +0x1C / +0x14 | `0x1A0134` (`sub_1A0134`) | indexed |
| 6 | +0x20 / +0x18 | `0x1A0138` (`sub_1A0138`) | indexed |
| 7 | +0x24 / +0x1C | `0x1A013C` (`sub_1A013C`) | indexed |
| 8 | +0x28 / +0x20 | `0x1A0140` (`sub_1A0140`) | indexed |
| 9 | +0x2C / +0x24 | `0x1A0144` (`sub_1A0144`) | indexed |
| 10 | +0x30 / +0x28 | `0x1A0708` (`sub_1A0708`) | indexed |

## `vtable for'TrinitiAnalyticsJsonObject`

Vtable at `0x201E20` (symbol `_ZTV26TrinitiAnalyticsJsonObject`), 11 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1A081C` (`sub_1A081C`) | indexed |
| 1 | +0xC / +0x4 | `0x1A077C` (`sub_1A077C`) | indexed |
| 2 | +0x10 / +0x8 | `0x1A0920` (`sub_1A0920`) | indexed |
| 3 | +0x14 / +0xC | `0x1A012C` (`sub_1A012C`) | indexed |
| 4 | +0x18 / +0x10 | `0x1A0130` (`sub_1A0130`) | indexed |
| 5 | +0x1C / +0x14 | `0x1A092C` (`sub_1A092C`) | indexed |
| 6 | +0x20 / +0x18 | `0x1A0138` (`sub_1A0138`) | indexed |
| 7 | +0x24 / +0x1C | `0x1A013C` (`sub_1A013C`) | indexed |
| 8 | +0x28 / +0x20 | `0x1A0140` (`sub_1A0140`) | indexed |
| 9 | +0x2C / +0x24 | `0x1A0144` (`sub_1A0144`) | indexed |
| 10 | +0x30 / +0x28 | `0x1A0988` (`sub_1A0988`) | indexed |

## `vtable for'TrinitiAnalyticsJsonArray`

Vtable at `0x201E60` (symbol `_ZTV25TrinitiAnalyticsJsonArray`), 11 slots.

| Slot | Offset from symbol / from vptr | Function | In core set |
|---:|---|---|---|
| 0 | +0x8 / +0x0 | `0x1A0DB8` (`sub_1A0DB8`) | indexed |
| 1 | +0xC / +0x4 | `0x1A0D18` (`sub_1A0D18`) | indexed |
| 2 | +0x10 / +0x8 | `0x1A0EA8` (`sub_1A0EA8`) | indexed |
| 3 | +0x14 / +0xC | `0x1A012C` (`sub_1A012C`) | indexed |
| 4 | +0x18 / +0x10 | `0x1A0130` (`sub_1A0130`) | indexed |
| 5 | +0x1C / +0x14 | `0x1A0134` (`sub_1A0134`) | indexed |
| 6 | +0x20 / +0x18 | `0x1A0EB4` (`sub_1A0EB4`) | indexed |
| 7 | +0x24 / +0x1C | `0x1A013C` (`sub_1A013C`) | indexed |
| 8 | +0x28 / +0x20 | `0x1A0140` (`sub_1A0140`) | indexed |
| 9 | +0x2C / +0x24 | `0x1A0144` (`sub_1A0144`) | indexed |
| 10 | +0x30 / +0x28 | `0x1A0EBC` (`sub_1A0EBC`) | indexed |

