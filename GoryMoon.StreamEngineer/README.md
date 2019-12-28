# StreamEngineer

To get started using this plugin you need to add the path to the launch options of Space Engineers.

Extract this folder anywhere and get it's path, save this for later.

 1. Go to `Space Engineers` in steam. 
 2. Open the properties of it.
 3. Click `SET LAUNCH OPTIONS...`
 4. Add `-plugin <path>/StreamEngineer.dll` where `<path>` is the one from above 
 5. Open the `settings.toml` and enter the `SteamName` 
 6. Open the `token-settings.toml` and enter the `StreamLabsToken`
 
## Twitch Extension

Go to the twitch extensions and search for `Stream Engineer` and add it.

In the configuration of the extension you need to copy the `Mod Token` into the `token-settings.toml` file.

## Configuring events

To edit the events open the `events.json`, can be done while game is running.

### Actions
|           Type           | Comments                                                                                       |              Additional parameters                        |
|:------------------------:|------------------------------------------------------------------------------------------------|:---------------------------------------------------------:|
|        `"meteors"`       | Spawns `amount` of meteor storms, `radius` of `1.0` is like vanilla, lower is closer to player | `"radius": 1.0, "amount": "number" or "event"`            |
|     `"toggle_power"`     | Turns on the power of the current vehicle                                                      |                                                           |
|     `"disable_power"`    | Turns off the power of the current vehicle                                                     |                                                           |
|     `"toggle_power"`     | Toggles the power of the current vehicle                                                       |                                                           |
|        `"refill"`        | Refills health, O2, H2 and energy                                                              |                                                           |
|       `"power_up"`       | Refills the batteries in the current vehicle and player                                        |                                                           |
|      `"power_down"`      | Empties the batteries in the current vehicle and player                                        | `"amount": -1` empties, other values removes, in MWh      |
|   `"toggle_dampeners"`   | Toggles the dampeners                                                                          |                                                           |
|   `"enable_dampeners"`   | Turns the dampeners on                                                                         |                                                           |
|   `"disable_dampeners"`  | Turns the dampeners off                                                                        |                                                           |
|   `"toggle_thrusters"`   | Toggles the thrusters                                                                          |                                                           |
|   `"enable_thrusters"`   | Turns the thrusters on                                                                         |                                                           |
|   `"disable_thrusters"`  | Turns the thrusters off                                                                        |                                                           |
|    `"toggle_helmet"`     | Toggles the helmet                                                                             |                                                           |
|    `"enable_helmet"`     | Turns opens the helmet                                                                         |                                                           |
|    `"disable_helmet"`    | Turns closes the helmet                                                                        |                                                           |
| `"fulfill_buildplanner"` | Gives the player all items that fit from the current build planner                             |                                                           |
|        `"random"`        | Runs a random provided action                                                                  | `"actions": []"`, example down below                      |
|       `"warhead"`        | Spawns a warhead above the player at a given range, speed and countdown                        | `"speed, distance, countdown": "number" or "event", "space_speed, space_distance, space_countdown": "number" or "event",  "hostile": true or false, ` |
|      `"give_item"`       | Gives the defined item/items to the player, IDs are below                                      | `"items": [{"id": "ID", "amount": "number" or " event"}]` |
|   `"inventory_bomb"`     | Drops all item in the players inventory                                                        |                                                           |
|     `"spawn_drone"`      | Spawns a random drone or a specified one                                                       | `"drone": "id of drone"`, optional, find id below         |
|         `"snap"`         | Does the Thanos Snap, WARNING! Can be very destructive, can set snap check percentage          | `"vehicle": true or false`, does a snap on all cubes, `"vehicle_percentage": 0.5`, `"player_percentage": 0.5` |
|  `"charge_jump_drive"`   | Fully charges all jump drives in the current vehicle                                           |                                                           |

`event` can be used on supported parameters to use the amount data from the event. 
For example the amount of months a user have subscribed.
For `twitch_subscription` you can use `tier` as a parameter to calculate amount, the `tier` value can be `1`, `2` or `3`, prime sub is also `1`

The warhead have different settings for space or a planet, if only planet settings are set those are used for both. 
By default the warhead is owned by the player.

### Conditions

You can put conditions on events to only run when those are met, you can have multiple conditions and only one of them needs to be met.

All parameters in a condition is optional except the `type` parameter. 
Not setting one of `to` or `from` will match up to or from to everything depending on which one.

You can leave the condition array empty to always run the action.

> Example conditions
```json
{
  "type": "twitch_bits",
  "from": 100,
  "to": 500
}
```

Only affect tier 3 twitch subscriptions
```json
{
  "type": "twitch_subscription_tier",
  "from": 3
}
```

Only affect when a specific channel point reward is run, get the id from https://seapi.gorymoon.se/dashboard/chat
```json
{
  "type": "twitch_channel_points",
  "channel_point_id": "e6149787-6bcc-4bbc-bdd9-c360de68c956"
}
```

The available types are:

|          Type               |            `event`            |
|:---------------------------:|:-----------------------------:|
| `donation`                  | Donation amount               |
| `twitch_subscription`       | Months subscribed             |
| `twitch_subscription_tier`  | The tier, `1`,`2` or `3`      |
| `twitch_bits`               | The amount of bits            |
| `twitch_follow`             |                               |
| `twitch_host`               | The amount of viewers hosted  |
| `twitch_raid`               | The amount of viewers raiding |
| `twitch_channel_points`     |                               |
| `youtube_subscription`      |                               |
| `youtube_sponsor`           | Months sponsored              |
| `youtube_superchat`         | The amount superchatted       |
| `mixer_follow`              |                               |
| `mixer_subscription`        | Months subscribed             |
| `mixer_host`                | The amount of viewers hosted  |

### Give Item Action

The Amount with the item can have decimals for some items and not for others, it's listed below

Example of the items definition
```json
{
  "items": [
    {
      "id": "Ore/Iron",
      "amount": "0.5"
    },
    {
      "id": "Component/SteelPlate",
      "amount": "50"
    }
  ]
}
```

| Id                                            | Display Name                 | Notes              |
|-----------------------------------------------|------------------------------|--------------------|
| Ore/Stone                                     | Stone                        | Can have Decimals  |
| Ore/Iron                                      | Iron Ore                     | Can have Decimals  |
| Ore/Nickel                                    | Nickel Ore                   | Can have Decimals  |
| Ore/Cobalt                                    | Cobalt Ore                   | Can have Decimals  |
| Ore/Magnesium                                 | Magnesium Ore                | Can have Decimals  |
| Ore/Silicon                                   | Silicon Ore                  | Can have Decimals  |
| Ore/Silver                                    | Silver Ore                   | Can have Decimals  |
| Ore/Gold                                      | Gold Ore                     | Can have Decimals  |
| Ore/Platinum                                  | Platinum Ore                 | Can have Decimals  |
| Ore/Uranium                                   | Uranium Ore                  | Can have Decimals  |
| Ore/Scrap                                     | Scrap Metal                  | Can have Decimals  |
| Ore/Ice                                       | Ice Ore                      | Can have Decimals  |
|                                               |                              |                    |
| Ingot/Stone                                   | Gravel                       | Can have Decimals  |
| Ingot/Iron                                    | Iron Ingot                   | Can have Decimals  |
| Ingot/Nickel                                  | Nickel Ingot                 | Can have Decimals  |
| Ingot/Cobalt                                  | Cobalt Ingot                 | Can have Decimals  |
| Ingot/Magnesium                               | Magnesium Powder             | Can have Decimals  |
| Ingot/Silicon                                 | Silicon Wafer                | Can have Decimals  |
| Ingot/Silver                                  | Silver Ingot                 | Can have Decimals  |
| Ingot/Gold                                    | Gold Ingot                   | Can have Decimals  |
| Ingot/Platinum                                | Platinum Ingot               | Can have Decimals  |
| Ingot/Uranium                                 | Uranium Ingot                | Can have Decimals  |
| Ingot/Scrap                                   | Old Scrap Metal              | Can have Decimals  |
|                                               |                              |                    |
| PhysicalGunObject/AutomaticRifleItem          | Automatic Rifle              | No Decimals        |
| PhysicalGunObject/PreciseAutomaticRifleItem   | Precise Automatic Rifle      | No Decimals        |
| PhysicalGunObject/RapidFireAutomaticRifleItem | Rapid\-Fire Automatic Rifle  | No Decimals        |
| PhysicalGunObject/UltimateAutomaticRifleItem  | Elite Automatic Rifle        | No Decimals        |
| PhysicalGunObject/WelderItem                  | Welder                       | No Decimals        |
| PhysicalGunObject/Welder2Item                 | Enhanced Welder              | No Decimals        |
| PhysicalGunObject/Welder3Item                 | Proficient Welder            | No Decimals        |
| PhysicalGunObject/Welder4Item                 | Elite Welder                 | No Decimals        |
| PhysicalGunObject/AngleGrinderItem            | Grinder                      | No Decimals        |
| PhysicalGunObject/AngleGrinder2Item           | Enhanced Grinder             | No Decimals        |
| PhysicalGunObject/AngleGrinder3Item           | Proficient Grinder           | No Decimals        |
| PhysicalGunObject/AngleGrinder4Item           | Elite Grinder                | No Decimals        |
| PhysicalGunObject/HandDrillItem               | Hand Drill                   | No Decimals        |
| PhysicalGunObject/HandDrill2Item              | Enhanced Hand Drill          | No Decimals        |
| PhysicalGunObject/HandDrill3Item              | Proficient Hand Drill        | No Decimals        |
| PhysicalGunObject/HandDrill4Item              | Elite Hand Drill             | No Decimals        |
|                                               |                              |                    |
| OxygenContainerObject/OxygenBottle            | Oxygen Bottle                | Empty, No Decimals |
| GasContainerObject/HydrogenBottle             | Hydrogen Bottle              | Empty, No Decimals |
|                                               |                              |                    |
| ConsumableItem/ClangCola                      | Clang Kola                   | No Decimals        |
| ConsumableItem/CosmicCoffee                   | Cosmic Coffee                | No Decimals        |
| ConsumableItem/Medkit                         | Medkit                       | No Decimals        |
| ConsumableItem/Powerkit                       | Powerkit                     | No Decimals        |
|                                               |                              |                    |
| AmmoMagazine/NATO\_5p56x45mm                  | 5\.56x45mm NATO magazine     | No Decimals        |
| AmmoMagazine/NATO\_25x184mm                   | 25x184mm NATO ammo container | No Decimals        |
| AmmoMagazine/Missile200mm                     | 200mm missile container      | No Decimals        |
|                                               |                              |                    |
| Component/Construction                        | Construction Comp\.          | No Decimals        |
| Component/MetalGrid                           | Metal Grid                   | No Decimals        |
| Component/InteriorPlate                       | Interior Plate               | No Decimals        |
| Component/SteelPlate                          | Steel Plate                  | No Decimals        |
| Component/Girder                              | Girder                       | No Decimals        |
| Component/SmallTube                           | Small Steel Tube             | No Decimals        |
| Component/LargeTube                           | Large Steel Tube             | No Decimals        |
| Component/Motor                               | Motor                        | No Decimals        |
| Component/Display                             | Display                      | No Decimals        |
| Component/BulletproofGlass                    | Bulletproof Glass            | No Decimals        |
| Component/Superconductor                      | Superconductor               | No Decimals        |
| Component/Computer                            | Computer                     | No Decimals        |
| Component/Reactor                             | Reactor Comp\.               | No Decimals        |
| Component/Thrust                              | Thruster Comp\.              | No Decimals        |
| Component/GravityGenerator                    | Gravity Comp\.               | No Decimals        |
| Component/Medical                             | Medical Comp\.               | No Decimals        |
| Component/RadioCommunication                  | Radio\-comm Comp\.           | No Decimals        |
| Component/Detector                            | Detector Comp\.              | No Decimals        |
| Component/Explosives                          | Explosives                   | No Decimals        |
| Component/SolarCell                           | Solar Cell                   | No Decimals        |
| Component/PowerCell                           | Power Cell                   | No Decimals        |
| Component/Canvas                              | Canvas                       | No Decimals        |
| Component/ZoneChip                            | Zone Chip                    | No Decimals        |

### Spawn Drone ids

The action can take a specific drone to spawn or use a random one from the config file.
If the drone id isn't correct it won't spawn anything.

The drones must be of the type `SpawnGroupDefinition`, for custom drones you need to find the ids of those

| Id                         | Type         |
|----------------------------|--------------|
| Blue Drone MK1             | Space        |
| Blue Drone MK2             | Space        |
| Red Drone MK1              | Space        |
| Light patrol drone         | Space        |
| Escord Drone               | Space        |
| Vulture Drone              | Space/Planet |
| ProtectoBot                | Space        |
| Raider Drone               | Space        |
| Salvage Drone              | Space        |
| Seeker Mine                | Space        |
| Snub Fighter               | Space        |
| Stash-satellite            | Space        |
| V2-Gunboat                 | Space        |
| Hostile Miner              | Space        |
| Tusk                       | Space        |
| DroneS 1GG_1               | Space        |
| DroneS 1GG_2               | Space        |
| DroneS 1GG_3               | Space        |
| DroneS Drill.Warhead       | Space        |
| DroneS 2GG.1GT             | Space        |
| DroneS 1MT.2GG             | Space        |
| DroneS 2GG.1GT.1RL         | Space        |
| DroneS 2GG.2GT             | Space        |
| DroneL 1RL.1GG.2IT         | Space        |
| BossDroneL 1RL.2GT.1MT.2IT | Space        |

### Example

You can have multiple of the same event with different conditions

```json
[
  {
    "type": "meteors",
    "action": {
      "conditions": [
        {
          "type": "twitch_bits",
          "from": 100,
          "to": 500
        }
      ],
      "message": "Let it RAIN!",
      "radius": 1.0,
      "amount": "1"
    }
  },
  {
    "type": "meteors",
    "action": {
      "conditions": [
        {
          "type": "twitch_subscription",
          "from": 6
        }
      ],
      "message": "Let it RAIN!",
      "radius": 0.2,
      "amount": "event / 5"
    }
  }
]
```

#### Random action

`action` contains a list of actions to pick from. 

You can set the `weight` on each item to make on more probable, setting the to the same will make it pick equally.
Setting no weight value will default it to `1`.
```json
[
  {
    "type": "random",
    "action": {
      "conditions": [
        {
          "type": "twitch_bits",
          "from": 100
        }
      ],
      "actions": [
        {
          "type": "meteors",
          "weight": 1,
          "action": {
            "message": "Close call!",
            "amount": 1,
            "radius": 0.0
          }
        },
        {
          "type": "power_toggle",
          "weight": 4,
          "action": {}
        },
        {
          "type": "meteors",
          "weight": 3,
          "action": {
            "message": "Let it RAIN!",
            "amount": 2,
            "radius": 1.0
          }
        }
      ]
    }
  }
]
```
