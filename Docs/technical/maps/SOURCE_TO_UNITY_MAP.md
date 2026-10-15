# Source to Unity map

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

Relations between original functions and the port's code in this tree. Two sources: the 12 hand-mapped symbols in the deep dossiers (reviewed), and every comment in the Unity code that cites a `sub_XXXX` address (**mechanical: a comment that cites an address is a reference, not proof that the code implements that function**). Many-to-many; a core function with no row is **unmapped**, not absent.

## Mapped by hand (deep dossiers)

| Native | Unity location | Role |
|---|---|---|
| `0x17C950` sub_17C950 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:1347` | pause flag and scene clock |
| `0x17C950` sub_17C950 | `Assets/DinoCap/PortV2/Runtime/Power2DSourceTruthRenderer.cs:267` | renderer pause flag |
| `0x17BA5C` sub_17BA5C | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:1377` | scene clock and pause gate |
| `0x17BA5C` sub_17BA5C | `Assets/DinoCap/PortV2/Runtime/Power2DSourceTruthRenderer.cs:784` | renderer freeze |
| `0x666BC` sub_666BC | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:5101` | melee start |
| `0x666BC` sub_666BC | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:5411` | melee press flag |
| `0x66550` sub_66550 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:1443` | completion re-arm |
| `0x68148` sub_68148 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:4836` | damage entry |
| `0xCE2C` sub_CE2C | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:2751` | door range |
| `0x6F124` sub_6F124 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:3854` | level clear |
| `0x6F124` sub_6F124 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:3929` | shop exit and increment |
| `0xFE50` sub_FE50 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:6640` | attack cone |
| `0x7EB04` sub_7EB04 | `Assets/DinoCap/PortV2/Runtime/NativeAudioSessionGate.cs:102` | sound gate |
| `0x165298` sub_165298 | `Assets/DinoCap/PortV2/Runtime/NativeAudioSessionGate.cs:102` | session cap |
| `0x56E94` sub_56E94 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:2613` | touch layer |
| `0x56E94` sub_56E94 | `Assets/DinoCap/PortV2/Runtime/CityGridController.cs:5414` | touch end |
| `0x76A20` sub_76A20 | `Assets/DinoCap/PortV2/Runtime/Power2DSourceTruthRenderer.cs:105` | animation tick |
| `0x76A20` sub_76A20 | `Assets/DinoCap/PortV2/Runtime/Power2DSourceTruthRenderer.cs:776` | pause freeze |

## Mapped by code comments (162 distinct addresses cited, 621 citations)

| Native | Core? | Citations in Unity code |
|---|---|---|
| `0xBF34` | indexed | `Runtime/CityGridController.cs:3555`, `Runtime/CityGridController.cs:8328` |
| `0xC990` | indexed | `Runtime/CityGridController.cs:3145` |
| `0xCD58` | yes | `Runtime/CityGridController.cs:1083` |
| `0xCE2C` | yes | `Runtime/CityGridController.cs:2693`, `Runtime/CityGridController.cs:2741` |
| `0xCF14` | yes | `Runtime/CityGridController.cs:199`, `Runtime/CityGridController.cs:4428` |
| `0xD004` | yes | `Runtime/CityGridController.cs:3011`, `Runtime/CityGridController.cs:3186` |
| `0xD308` | indexed | `Runtime/CityGridController.cs:133`, `Runtime/CityGridController.cs:976` |
| `0xDAA0` | indexed | `Runtime/Combat/DinoCapDamageParticle.cs:254`, `Runtime/Combat/DinoCapDamageParticle.cs:281`, `Runtime/Combat/FlyingHeadParticle.cs:188`, `Runtime/Combat/WeaponSuirouParticle.cs:93` |
| `0xEFB4` | yes | `Runtime/CityGridController.cs:3172` |
| `0xF154` | yes | `Runtime/CityGridController.cs:3012`, `Runtime/CityGridController.cs:3171` |
| `0xFD58` | yes | `Runtime/CityGridController.cs:448`, `Runtime/CityGridController.cs:4319`, `Runtime/CityGridController.cs:6472`, `Runtime/CityGridController.cs:6568`, `Runtime/CityGridController.cs:6602` ... (13) |
| `0xFDE0` | yes | `Runtime/CityGridController.cs:6470`, `Runtime/CityGridController.cs:6478`, `Runtime/CombatSandboxV2.cs:158`, `Runtime/CombatSandboxV2.cs:1772`, `Runtime/CombatSandboxV2.cs:1781` |
| `0xFE50` | yes | `Runtime/CityGridController.cs:6597`, `Runtime/CityGridController.cs:6607`, `Runtime/CityGridController.cs:6609` |
| `0xFEF0` | indexed | `Runtime/CombatSandboxV2.cs:1915` |
| `0xFF68` | indexed | `Runtime/CombatSandboxV2.cs:2486` |
| `0x100B8` | indexed | `Runtime/CityGridController.cs:6475` |
| `0x101A0` | yes | `Runtime/CityGridController.cs:4136`, `Runtime/CityGridController.cs:5593`, `Runtime/CityGridController.cs:6287`, `Runtime/CityGridController.cs:6645`, `Runtime/CombatSandboxV2.cs:411` ... (8) |
| `0x10294` | yes | `Runtime/CityGridController.cs:5720`, `Runtime/CityGridController.cs:6112`, `Runtime/CombatSandboxV2.cs:487`, `Runtime/CombatSandboxV2.cs:1923`, `Runtime/CombatSandboxV2.cs:2072` ... (7) |
| `0x10554` | yes | `Runtime/CityGridController.cs:639`, `Runtime/CityGridController.cs:4380`, `Runtime/CityGridController.cs:4665`, `Runtime/CityGridController.cs:4709`, `Runtime/CityGridController.cs:4710` ... (33) |
| `0x10B90` | yes | `Runtime/CityGridController.cs:4310`, `Runtime/CityGridController.cs:4328`, `Runtime/CityGridController.cs:6318`, `Runtime/Combat/DinoFatalDeathCore.cs:5`, `Runtime/CombatSandboxV2.cs:3667` ... (7) |
| `0x10ECC` | yes | `Runtime/Combat/DinoCapBloodBurst.cs:6`, `Runtime/Combat/DinoCapBloodBurst.cs:15`, `Runtime/Combat/DinoCapBloodBurst.cs:90`, `Runtime/Combat/DinoCapBloodBurst.cs:99`, `Runtime/CombatSandboxV2.cs:2483` ... (17) |
| `0x11124` | yes | `Runtime/CityGridController.cs:4665`, `Runtime/CityGridController.cs:4714`, `Runtime/Combat/DinoCapBloodBurst.cs:8`, `Runtime/Combat/DinoCapDamageParticle.cs:35`, `Runtime/Combat/DinoCapDamageParticle.cs:103` ... (10) |
| `0x1141C` | yes | `Runtime/CityGridController.cs:4290`, `Runtime/CityGridController.cs:4561`, `Runtime/CityGridController.cs:4582`, `Runtime/Combat/DinoFatalDeathCore.cs:6`, `Runtime/Combat/DinoFatalDeathCore.cs:54` ... (14) |
| `0x118E8` | yes | `Runtime/CityGridController.cs:6319`, `Runtime/Combat/DinoFatalDeathCore.cs:138`, `Runtime/CombatSandboxV2.cs:3647`, `Runtime/CombatSandboxV2.cs:3665`, `Runtime/CombatSandboxV2.cs:3668` ... (7) |
| `0x11980` | yes | `Runtime/CityGridController.cs:4289`, `Runtime/CityGridController.cs:4302`, `Runtime/CityGridController.cs:4311`, `Runtime/CityGridController.cs:4312`, `Runtime/CityGridController.cs:4622` ... (11) |
| `0x12884` | yes | `Runtime/CityGridController.cs:6267`, `Runtime/CombatSandboxV2.cs:411`, `Runtime/CombatSandboxV2.cs:2483`, `Runtime/CombatSandboxV2.cs:3575`, `Runtime/CombatSandboxV2.cs:3791` |
| `0x12C64` | yes | `Runtime/CityGridController.cs:6299`, `Runtime/CombatSandboxV2.cs:411`, `Runtime/CombatSandboxV2.cs:485`, `Runtime/CombatSandboxV2.cs:2602`, `Runtime/CombatSandboxV2.cs:3614` ... (7) |
| `0x12F28` | yes | `Runtime/Combat/F2FlameVisual.cs:79`, `Runtime/Combat/F3LaserVisual.cs:10`, `Runtime/Combat/F3LaserVisual.cs:25`, `Runtime/Combat/F3LaserVisual.cs:82`, `Runtime/Combat/F3LaserVisual.cs:85` ... (7) |
| `0x13FD4` | yes | `Runtime/CityGridController.cs:3068` |
| `0x18858` | yes | `Runtime/CombatSandboxV2.cs:2486`, `Runtime/JungleSpawnPlacement.cs:40` |
| `0x19340` | indexed | `Runtime/CityGridController.cs:2241` |
| `0x1BDC0` | yes | `Runtime/CityGridController.cs:7858`, `Runtime/Power2DSourceTruthRenderer.cs:181` |
| `0x2A8A8` | yes | `Runtime/CityGridController.cs:262`, `Runtime/CityGridController.cs:3077`, `Runtime/CityGridController.cs:8229`, `Runtime/Combat/DinoCapBloodBurst.cs:37`, `Runtime/Combat/DinoCapBloodBurst.cs:51` ... (9) |
| `0x53088` | indexed | `Runtime/CityGridController.cs:7887` |
| `0x53330` | yes | `Runtime/CityGridController.cs:127`, `Runtime/CityGridController.cs:396`, `Runtime/CityGridController.cs:443`, `Runtime/CityGridController.cs:857`, `Runtime/CityGridController.cs:859` ... (19) |
| `0x54FA0` | yes | `Runtime/CityGridController.cs:381`, `Runtime/CityGridController.cs:858`, `Runtime/CityGridController.cs:1164`, `Runtime/CityGridController.cs:1182`, `Runtime/CityGridController.cs:1201` ... (6) |
| `0x55B1C` | yes | `Runtime/CityGridController.cs:3274`, `Runtime/CityGridController.cs:3290`, `Runtime/CityGridController.cs:3386`, `Runtime/CityGridController.cs:3397`, `Runtime/CityGridController.cs:7757` ... (6) |
| `0x56030` | yes | `Runtime/CityGridController.cs:2693`, `Runtime/CityGridController.cs:2741`, `Runtime/CityGridController.cs:2933` |
| `0x56118` | yes | `Runtime/CityGridController.cs:7871` |
| `0x56450` | yes | `Runtime/CityGridController.cs:805`, `Runtime/CityGridController.cs:820`, `Runtime/CityGridController.cs:823` |
| `0x56ACC` | yes | `Runtime/CityGridController.cs:8505` |
| `0x56E94` | yes | `Runtime/CityGridController.cs:2367`, `Runtime/CityGridController.cs:2376`, `Runtime/CityGridController.cs:2671`, `Runtime/CityGridController.cs:5404`, `Runtime/CityGridController.cs:5416` ... (8) |
| `0x5744C` | yes | `Runtime/CityGridController.cs:550`, `Runtime/CityGridController.cs:4008`, `Runtime/CityGridController.cs:4012`, `Runtime/CityGridController.cs:4795`, `Runtime/CombatSandboxV2.cs:4920` |
| `0x57BD8` | yes | `Runtime/CityGridController.cs:4007`, `Runtime/CityGridController.cs:4011`, `Runtime/CityGridController.cs:4047`, `Runtime/CityGridController.cs:4194`, `Runtime/CityGridController.cs:6477` ... (7) |
| `0x584D4` | indexed | `Runtime/CombatSandboxV2.cs:2969`, `Runtime/CombatSandboxV2.cs:2970`, `Runtime/CombatSandboxV2.cs:2983` |
| `0x58B5C` | yes | `Runtime/CityGridController.cs:3149`, `Runtime/CityGridController.cs:8213` |
| `0x5931C` | yes | `Runtime/CityGridController.cs:2229` |
| `0x59DEC` | yes | `Runtime/CityGridController.cs:4808`, `Runtime/MainMenuController.cs:11`, `Runtime/MainMenuController.cs:215` |
| `0x5D064` | indexed | `Runtime/MainMenuController.cs:212` |
| `0x5D380` | indexed | `Runtime/CityGridController.cs:6352`, `Runtime/CityGridController.cs:6352`, `Runtime/CityGridController.cs:8601`, `Runtime/CityGridController.cs:8601`, `Runtime/CityGridController.cs:8605` ... (7) |
| `0x5EEEC` | yes | `Runtime/CityGridController.cs:6351`, `Runtime/CityGridController.cs:7991`, `Runtime/CityGridController.cs:8598`, `Runtime/CityGridController.cs:8600`, `Runtime/CityGridController.cs:8604` ... (6) |
| `0x5FBA8` | yes | `Runtime/MainMenuController.cs:592` |
| `0x5FC00` | yes | `Runtime/MainMenuController.cs:592` |
| `0x5FD70` | indexed | `Runtime/CityGridController.cs:1911`, `Runtime/MainMenuController.cs:80`, `Runtime/MainMenuController.cs:654` |
| `0x604B0` | indexed | `Runtime/MainMenuController.cs:87` |
| `0x61AD4` | indexed | `Runtime/MainMenuController.cs:518` |
| `0x61D40` | indexed | `Runtime/MainMenuController.cs:99` |
| `0x622E8` | indexed | `Runtime/MainMenuController.cs:99` |
| `0x627CC` | yes | `Runtime/MainMenuController.cs:263`, `Runtime/MainMenuController.cs:304` |
| `0x62A98` | yes | `Runtime/CityGridController.cs:3084`, `Runtime/MainMenuController.cs:470` |
| `0x63BB0` | yes | `Runtime/CityGridController.cs:8401`, `Runtime/CityGridController.cs:8440` |
| `0x64694` | yes | `Runtime/CityGridController.cs:1758`, `Runtime/CityGridController.cs:2304`, `Runtime/CityGridController.cs:8402`, `Runtime/CityGridController.cs:8428`, `Runtime/CityGridController.cs:8458` ... (8) |
| `0x64FA4` | yes | `Runtime/CityGridController.cs:1768`, `Runtime/CityGridController.cs:7904` |
| `0x65A08` | yes | `Runtime/CityGridController.cs:5310`, `Runtime/CityGridController.cs:6415`, `Runtime/CombatSandboxV2.cs:335`, `Runtime/CombatSandboxV2.cs:2338` |
| `0x65C78` | yes | `Runtime/CityGridController.cs:568`, `Runtime/CityGridController.cs:1396`, `Runtime/CityGridController.cs:4948` |
| `0x65D1C` | yes | `Runtime/CityGridController.cs:1514`, `Runtime/JungleSpawnPlacement.cs:40` |
| `0x65E2C` | yes | `Runtime/CityGridController.cs:496`, `Runtime/CityGridController.cs:502`, `Runtime/CityGridController.cs:1541`, `Runtime/CityGridController.cs:3410` |
| `0x66110` | yes | `Runtime/CityGridController.cs:568`, `Runtime/CityGridController.cs:5309` |
| `0x66310` | yes | `Runtime/CityGridController.cs:2824`, `Runtime/CityGridController.cs:4866`, `Runtime/CombatSandboxV2.cs:2610` |
| `0x663A0` | yes | `Runtime/CityGridController.cs:4384`, `Runtime/CityGridController.cs:5309`, `Runtime/CityGridController.cs:5558`, `Runtime/CityGridController.cs:5577`, `Runtime/CityGridController.cs:5589` ... (16) |
| `0x66550` | yes | `Runtime/CityGridController.cs:1438`, `Runtime/CityGridController.cs:1514`, `Runtime/CityGridController.cs:4794`, `Runtime/CityGridController.cs:5143`, `Runtime/CityGridController.cs:5407` ... (10) |
| `0x666BC` | yes | `Runtime/CityGridController.cs:1515`, `Runtime/CityGridController.cs:2368`, `Runtime/CityGridController.cs:5104`, `Runtime/CityGridController.cs:5140`, `Runtime/CityGridController.cs:5143` ... (7) |
| `0x667B0` | yes | `Runtime/CityGridController.cs:1441`, `Runtime/CityGridController.cs:1442`, `Runtime/CityGridController.cs:1515`, `Runtime/CityGridController.cs:2368`, `Runtime/CityGridController.cs:4385` ... (18) |
| `0x668FC` | yes | `Runtime/Combat/MeleeContactCore.cs:7`, `Runtime/CombatSandboxV2.cs:1184`, `Runtime/CombatSandboxV2.cs:2649`, `Runtime/CombatSandboxV2.cs:2750`, `Runtime/CombatSandboxV2.cs:3342` ... (6) |
| `0x66BA4` | yes | `Runtime/CityGridController.cs:6435` |
| `0x66C18` | yes | `Runtime/CityGridController.cs:6435` |
| `0x66C74` | yes | `Runtime/CityGridController.cs:1448` |
| `0x66CE8` | yes | `Runtime/CityGridController.cs:5364`, `Runtime/CombatSandboxV2.cs:428`, `Runtime/CombatSandboxV2.cs:2577`, `Runtime/CombatSandboxV2.cs:3727` |
| `0x66DCC` | yes | `Runtime/CityGridController.cs:4390`, `Runtime/CityGridController.cs:4444`, `Runtime/CityGridController.cs:5280`, `Runtime/CityGridController.cs:5775`, `Runtime/CombatSandboxV2.cs:1168` ... (13) |
| `0x67884` | yes | `Runtime/CityGridController.cs:5794`, `Runtime/CombatSandboxV2.cs:3393` |
| `0x679F4` | yes | `Runtime/CityGridController.cs:662`, `Runtime/Combat/F2FlameVisual.cs:10`, `Runtime/Combat/F2FlameVisual.cs:28`, `Runtime/Combat/F2FlameVisual.cs:81`, `Runtime/CombatSandboxV2.cs:481` ... (10) |
| `0x67CE8` | yes | `Runtime/CityGridController.cs:663`, `Runtime/Combat/F3LaserVisual.cs:6`, `Runtime/Combat/F3LaserVisual.cs:24`, `Runtime/Combat/F3LaserVisual.cs:99`, `Runtime/CombatSandboxV2.cs:428` ... (9) |
| `0x68080` | yes | `Runtime/CityGridController.cs:2377`, `Runtime/CityGridController.cs:6433` |
| `0x68148` | yes | `Runtime/CityGridController.cs:567`, `Runtime/CityGridController.cs:1448`, `Runtime/CityGridController.cs:4839`, `Runtime/CityGridController.cs:4865`, `Runtime/CityGridController.cs:4871` ... (6) |
| `0x6833C` | yes | `Runtime/CityGridController.cs:569`, `Runtime/CityGridController.cs:4975`, `Runtime/CityGridController.cs:4994`, `Runtime/CityGridController.cs:5017`, `Runtime/Combat/DinoCapBloodBurst.cs:126` |
| `0x68A8C` | yes | `Runtime/CityGridController.cs:648`, `Runtime/CityGridController.cs:659`, `Runtime/CityGridController.cs:662`, `Runtime/CityGridController.cs:4336`, `Runtime/CityGridController.cs:5762` ... (11) |
| `0x68CD8` | yes | `Runtime/CityGridController.cs:1471`, `Runtime/CityGridController.cs:2802`, `Runtime/CityGridController.cs:2823`, `Runtime/CityGridController.cs:2824`, `Runtime/CityGridController.cs:2835` ... (7) |
| `0x69F30` | yes | `Runtime/CityGridController.cs:6366`, `Runtime/CityGridController.cs:7490` |
| `0x69FF0` | yes | `Runtime/CityGridController.cs:2691`, `Runtime/CityGridController.cs:6372`, `Runtime/CityGridController.cs:7495` |
| `0x6A4A8` | yes | `Runtime/CityGridController.cs:3513`, `Runtime/CityGridController.cs:3526`, `Runtime/CityGridController.cs:3552`, `Runtime/CityGridController.cs:3570`, `Runtime/CityGridController.cs:3588` ... (9) |
| `0x6CADC` | yes | `Runtime/CityGridController.cs:3569`, `Runtime/CityGridController.cs:3581`, `Runtime/CityGridController.cs:3604`, `Runtime/CityGridController.cs:7008`, `Runtime/CityGridController.cs:8108` ... (6) |
| `0x6F124` | yes | `Runtime/CityGridController.cs:3257`, `Runtime/CityGridController.cs:3275`, `Runtime/CityGridController.cs:3290`, `Runtime/CityGridController.cs:3411`, `Runtime/CityGridController.cs:3833` ... (7) |
| `0x6F748` | yes | `Runtime/CityGridController.cs:2083`, `Runtime/CityGridController.cs:2094`, `Runtime/CityGridController.cs:2131` |
| `0x7316C` | yes | `Runtime/CityGridController.cs:2091` |
| `0x7334C` | yes | `Runtime/CityGridController.cs:3275`, `Runtime/CityGridController.cs:3410`, `Runtime/CityGridController.cs:3840` |
| `0x764C4` | yes | `Runtime/CityGridController.cs:495`, `Runtime/CityGridController.cs:1083` |
| `0x7677C` | yes | `Runtime/CityGridController.cs:1084`, `Runtime/CombatSandboxV2.cs:2476`, `Runtime/CombatSandboxV2.cs:4177`, `Runtime/MainMenuController.cs:276`, `Runtime/MainMenuController.cs:279` |
| `0x76980` | yes | `Runtime/CityGridController.cs:4997`, `Runtime/Combat/F2FlameVisual.cs:10`, `Runtime/Combat/F3LaserVisual.cs:24`, `Runtime/Combat/F3LaserVisual.cs:25`, `Runtime/CombatSandboxV2.cs:412` ... (10) |
| `0x769D0` | yes | `Runtime/Power2DSourceTruthRenderer.cs:33`, `Runtime/Power2DSourceTruthRenderer.cs:83` |
| `0x76A20` | yes | `Runtime/CityGridController.cs:495`, `Runtime/CityGridController.cs:760`, `Runtime/CombatSandboxV2.cs:412`, `Runtime/CombatSandboxV2.cs:470`, `Runtime/CombatSandboxV2.cs:2042` ... (14) |
| `0x77498` | yes | `Runtime/CombatSandboxV2.cs:4583` |
| `0x77708` | yes | `Runtime/CityGridController.cs:2743` |
| `0x77A28` | yes | `Runtime/Combat/DinoCapBloodBurst.cs:27` |
| `0x780D8` | yes | `Runtime/Combat/DinoCapBloodBurst.cs:27` |
| `0x78658` | indexed | `Runtime/Combat/DinoCapBloodBurst.cs:27` |
| `0x78A38` | yes | `Runtime/Power2DSourceTruthRenderer.cs:576` |
| `0x78BE0` | yes | `Runtime/Power2DSourceTruthRenderer.cs:961` |
| `0x78D64` | yes | `Runtime/Combat/DinoCapBloodBurst.cs:35` |
| `0x7D554` | yes | `Runtime/Power2DSourceTruthRenderer.cs:575` |
| `0x7D81C` | yes | `Runtime/Power2DSourceTruthRenderer.cs:575` |
| `0x7E498` | yes | `Runtime/CityGridController.cs:1893`, `Runtime/CityGridController.cs:3837` |
| `0x7E748` | indexed | `Runtime/CityGridController.cs:3837` |
| `0x7E970` | yes | `Runtime/CityGridController.cs:1895`, `Runtime/CityGridController.cs:4810` |
| `0x7EB04` | yes | `Runtime/CombatSandboxV2.cs:1153`, `Runtime/CombatSandboxV2.cs:1170`, `Runtime/CombatSandboxV2.cs:3727` |
| `0x7EC50` | yes | `Runtime/CityGridController.cs:1762`, `Runtime/CityGridController.cs:1783` |
| `0x7EEC8` | yes | `Runtime/CityGridController.cs:3045`, `Runtime/CityGridController.cs:3066`, `Runtime/CityGridController.cs:3071`, `Runtime/CityGridController.cs:3073`, `Runtime/CityGridController.cs:3077` ... (9) |
| `0x7F934` | indexed | `Runtime/CityGridController.cs:805`, `Runtime/CityGridController.cs:820`, `Runtime/CityGridController.cs:823` |
| `0x85788` | yes | `Runtime/CityGridController.cs:4796` |
| `0x8925C` | indexed | `Runtime/Power2DSourceTruthRenderer.cs:181` |
| `0xA1518` | yes | `Runtime/CityGridController.cs:3077`, `Runtime/MainMenuController.cs:470`, `Runtime/Power2DSourceTruthRenderer.cs:182` |
| `0xC91D0` | yes | `Runtime/CityGridController.cs:4809` |
| `0xD05AC` | yes | `Runtime/CityGridController.cs:3084`, `Runtime/MainMenuController.cs:470` |
| `0xDA6D8` | indexed | `Runtime/NativeMoveButtonHost.cs:6` |
| `0xDA87C` | yes | `Runtime/MainMenuController.cs:105` |
| `0xE927C` | indexed | `Runtime/CityGridController.cs:3011`, `Runtime/CityGridController.cs:3029`, `Runtime/CityGridController.cs:3030`, `Runtime/CityGridController.cs:3040`, `Runtime/CityGridController.cs:3148` ... (6) |
| `0xE9628` | yes | `Runtime/MainMenuController.cs:293` |
| `0x1634F0` | yes | `Runtime/CityGridController.cs:496` |
| `0x16392C` | yes | `Runtime/CityGridController.cs:496` |
| `0x164440` | indexed | `Runtime/Power2DSourceTruthRenderer.cs:422` |
| `0x165298` | yes | `Runtime/CityGridController.cs:1765` |
| `0x166358` | yes | `Runtime/CityGridController.cs:495` |
| `0x166414` | yes | `Runtime/CityGridController.cs:496` |
| `0x16669C` | yes | `Runtime/CityGridController.cs:496` |
| `0x16A2CC` | indexed | `Runtime/Power2DSourceTruthRenderer.cs:421`, `Runtime/Power2DSourceTruthRenderer.cs:441`, `Runtime/Power2DSourceTruthRenderer.cs:472`, `Runtime/Power2DSourceTruthRenderer.cs:856` |
| `0x16BB3C` | yes | `Runtime/CityGridController.cs:1896` |
| `0x16BC64` | yes | `Runtime/CityGridController.cs:1895` |
| `0x16BE04` | yes | `Runtime/CityGridController.cs:1894` |
| `0x1706D4` | indexed | `Runtime/CityGridController.cs:3390` |
| `0x171E30` | yes | `Runtime/MainMenuController.cs:264` |
| `0x172BD0` | indexed | `Runtime/CityGridController.cs:445` |
| `0x172C0C` | yes | `Runtime/CityGridController.cs:862`, `Runtime/CityGridController.cs:864`, `Runtime/CityGridController.cs:1204`, `Runtime/CityGridController.cs:3572`, `Runtime/CityGridController.cs:3591` ... (9) |
| `0x172C90` | yes | `Runtime/CityGridController.cs:130`, `Runtime/CityGridController.cs:135`, `Runtime/CityGridController.cs:863`, `Runtime/CityGridController.cs:865`, `Runtime/CityGridController.cs:3594` |
| `0x172CC0` | yes | `Runtime/CityGridController.cs:135`, `Runtime/Power2DSourceTruthRenderer.cs:1478` |
| `0x172D50` | yes | `Runtime/CityGridController.cs:4056`, `Runtime/MainMenuController.cs:297` |
| `0x172DB8` | indexed | `Runtime/CityGridController.cs:3575` |
| `0x172DEC` | yes | `Runtime/CityGridController.cs:199`, `Runtime/CityGridController.cs:5308` |
| `0x173A98` | yes | `Runtime/CombatSandboxV2.cs:4178`, `Runtime/MainMenuController.cs:296`, `Runtime/MainMenuController.cs:328` |
| `0x173ABC` | indexed | `Runtime/CityGridController.cs:4057` |
| `0x173AF0` | indexed | `Runtime/Combat/DinoCapDamageParticle.cs:34` |
| `0x174780` | indexed | `Runtime/Combat/DinoCapBloodBurst.cs:45` |
| `0x175B44` | yes | `Runtime/CityGridController.cs:1764` |
| `0x17BA5C` | yes | `Runtime/Power2DSourceTruthRenderer.cs:422` |
| `0x17C720` | yes | `Runtime/MainMenuController.cs:264` |
| `0x17C840` | indexed | `Runtime/Combat/F3LaserVisual.cs:11` |
| `0x183D54` | indexed | `Runtime/CityGridController.cs:7772` |
| `0x183D94` | yes | `Runtime/CityGridController.cs:8429` |
| `0x183D9C` | indexed | `Runtime/CityGridController.cs:6376` |
| `0x185DCC` | indexed | `Runtime/CityGridController.cs:7715` |
| `0x18BB64` | yes | `Runtime/CityGridController.cs:7645`, `Runtime/CityGridController.cs:8112` |
| `0x18BBC4` | yes | `Runtime/CityGridController.cs:7642`, `Runtime/CityGridController.cs:7644`, `Runtime/CityGridController.cs:8108` |
| `0x18E2A4` | yes | `Runtime/CityGridController.cs:5308` |
| `0x190104` | indexed | `Runtime/CityGridController.cs:820` |

## Core functions with no mapping (68)

`0xD108`, `0xD9BC`, `0x12264`, `0x17AE4`, `0x1B13C`, `0x1B95C`, `0x5671C`, `0x59A54`, `0x61F10`, `0x63BAC`, `0x64030`, `0x65340`, `0x65988`, `0x661D8`, `0x6622C`, `0x69B48`, `0x69BA4`, `0x763FC`, `0x77DE4`, `0x781E4`, `0x78398`, `0x788E8`, `0x78B44`, `0x78BC8`, `0x79064`, `0x79B74`, `0x7EDA8`, `0x8161C`, `0xC0CE4`, `0xC8E38`, `0xD2904`, `0xD2AB8`, `0xD5BC0`, `0xDA2BC`, `0xDBC40`, `0xE0DA8`, `0xE4BA0`, `0x1635E0`, `0x164B78`, `0x16CC58`, `0x171808`, `0x171984`, `0x17208C`, `0x17251C`, `0x172554`, `0x172580`, `0x172C78`, `0x172C7C`, `0x172D14`, `0x172D18`, `0x172D1C`, `0x173B50`, `0x1742A0`, `0x1743A8`, `0x17457C`, `0x17566C`, `0x175B88`, `0x1783F0`, `0x178514`, `0x17C8B0`, `0x17C964`, `0x17DD28`, `0x17E5AC`, `0x17E9A0`, `0x17EC40`, `0x182058`, `0x183248`, `0x18B5D4`
