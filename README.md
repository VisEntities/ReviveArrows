This plugin enables players to shoot arrows that can heal and revive the wounded. Upon being hit by one of these arrows, it acts as a medical syringe, immediately restoring a portion of health as well as healing slowly over time. Additionally, if the player is wounded, the arrow has the capability to revive them. Should the healing be successful, the arrow's damage to the hit player will be nullified.

Once the player has the required ingredients in their inventory, as specified in the config, they should aim with their bow, hold down the use button, and then release the arrow while still holding the button down.

[Demonstration]()

-----------------------

## Permissions
- `revivearrows.use` - Allows to shoot healing arrows.

-----------------
## Configuration
```json
{
  "Version": "3.0.0",
  "Instant Health Increase": 15.0,
  "Health Increase Over Time": 20.0,
  "Can Revive Wounded": true,
  "Arrow Ingredients": [
    {
      "Shortname": "syringe.medical",
      "Amount": 1
    },
    {
      "Shortname": "rope",
      "Amount": 1
    }
  ]
}
```

------------------

## Localization
```json
{
  "InsufficientIngredients": "You don't have enough <color=#FABE28>{0}</color>. Required: <color=#FABE28>{1}</color>",
  "PlayerHealed ": "You healed <color=#FABE28>{0}</color> by <color=#FABE28>{1}</color> health points",
  "HealArrowUsage ": "Hold down <color=#FABE28>use</color> to heal a friend with an arrow"
}
```

-----------


## Credits
 * Rewritten from scratch and maintained to present by **VisEntities**
 * Originally created by **redBDGR**, up to version 1.0.1