# When your teammate is too far for a syringe!
In the heat of battle, every second counts. But when your teammate is injured, getting close enough to administer a syringe can mean risking your own life. Luckily, with ReviveArrows, you can heal your fallen allies from a safe distance without risking getting caught in the crossfire! Just aim, shoot, and watch as your teammate rises from the dead, ready to fight another day!

![](https://i.imgur.com/DbfZmgl.png)

---------

## Aim, hold, shoot!
In the chaos of battle, time is precious. You can't afford to waste it typing commands or fumbling through your inventory searching for a syringe. That's why revive arrows are designed to be as easy to use as regular arrows. You just switch between them, and here's how:

1. Ensure that you have the required materials in your inventory for revive arrows.
2. Aim with your bow like you normally would.
3. Before you release the arrow, hold down the revive arrow button and then shoot.
4. Voila! Your teammate is healed and ready to get back in the fight!

------

## Permissions
* `revivearrows.use` - Enables the use of revive arrows for healing injured players from a safe distance.

----------

## Configuration
```json
{
  "Version": "2.1.0",
  "Revive Arrow Button": "USE",
  "Instant Heal Amount": 15.0,
  "Heal Amount Over Time": 20.0,
  "Can Revive": true,
  "Revive Sound Effect": "assets/prefabs/tools/medical syringe/effects/inject_friend.prefab",
  "Consumable Items": [
    {
      "Item Shortname": "syringe.medical",
      "Amount To Consume": 1
    },
    {
      "Item Shortname": "rope",
      "Amount To Consume": 1
    }
  ]
}
```

------------------

## Localization
```json
{
  "NoPermission": "You lack the necessary permission to use heal arrows.",
  "Revive.Success": "Your arrow has done its job, the target has been healed!",
  "Revive.InsufficientConsumable": "Sorry, you cannot use revive arrows without the required materials!"
}
```

-----------

## Keep the mod alive
Creating plugins is my passion, and I love nothing more than exploring new ideas and bringing them to the community. But it takes hours of work every day to maintain and improve these plugins that you have come to love and rely on.

With your support on [Patreon](https://www.patreon.com/VisEntities), you're  giving me the freedom to devote more time and energy into what I love, which in turn allows me to continue providing new and exciting updates to the community.

![](https://i.imgur.com/8uhEWPb.png)

A portion of the contributions will also be donated to the uMod team as a token of appreciation for their dedication to coding quality, inspirational ideas, and time spent for the community.

------

## Credits
* Originally created by **Jake_Rich**, up to version 1.0.0
* Completely rewritten from scratch and maintained to present by **Dana**.
