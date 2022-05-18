# Development

---

## Long-term Versions

### v0.1: Groundwork

- [x] Camera which can be rotated / aimed
- [x] Setup for controlling specific object / passing control
- [x] Versatile stats class with modifiers / serialization
- [x] Capability for runes / scripts to affect stats (publicly accessible affectors)
- [x] General WorldObject as an interface for objects

### v0.2: Core Systems

- [x] Implement different movement classes which allow the user to move different
	- [x] MovementHover for standard hovering / aiming
	- [x] MovementHop for basic hopping along
- [x] System for crystal attaching / detaching from orbs
- [x] Refactor to use a generic Construct class which handles the interactions between the cores, runes, orbs
- [x] Rework PlayerController / PlayerCamera for better management of Forging mode as well as better interaction with Construct class
- [ ] Basic forging screen to allow better rune slotting / unslotting

### v0.3: Initial UX
- [ ] Add a generic / expandable system for object information for runes / orbs / cores
- [ ] Create a UI that dynamically shows information for hovered world objects - This will change and work correctly for each type of object information data
- [ ] Better camera scripts for clipping / moving in front of objects
- [ ] Expanding the interface for during forging:
	- [ ] Expanding tooltip / box on hovering
	- [ ] Highlighting rune slots when required

### v0.4: Initial Content
- [ ] Implement [[Objects#Destructible]] / damage system (for use with both objects as well as runes / enemies)
- [ ] Implement a skill system that runes can subscribe skills to
- [ ] [[Rune#Blaster]] weapon skill rune to test the destructable objects and skill system
- [ ] Basic health management for overall construct / individual orbs
	- [ ] Visible health bar UI
	- [ ] Generic Damage system including teams / quantity / potential stats inclusion

### v0.5: Expanding UI
- [ ] Dynamic UI for ingame:
	- [ ] Hotbar for skills
	- [ ] Resizing crosshair on hovering (This could include a unique animation for expanding around hovered objects)
- [ ] Some visual indicator of health for construct / orb onscreen
	...

### Backburner
- [ ] Implement inverse kinematics for [[Rune#Crawl]] movement rune to test the Construct classes ability to manage Movement classes

---

## Short-term Todo

- [x] Refactor code over to using new heirarchical construct system
	
	- [x] Introduce addition of skills and skillBindings
		- [x] Update core to use
		- [x] Update movementHop to use
	
	- [x] Update Movement to work with new system

- [x] Refactor all code to be clean, readable, extensible
	- Reformat to work with visual studio
	- Convert some to 1 line functions
	- Ensure public / private is all set correctly
		- Ensure correct naming of variables
	- Add headers for all serialized values

- [x] Clean up all todos

- [x] Rework PlayerController / PlayerCamera to use a state system for ingame / forging
	- [x] Link state between controller / camera

- [ ] Deal with physics better on hover attachment / detachment

- [ ] Change MovementHop AttackSkill particles size based on object

---

## Bugs / Issues

- [x] Hop no longer hopping

- [x] Can forge during detachment

- [ ] Rune slots have no collision

- [ ] MovementHover core jab can hit other objects

---

## Ideas

**Genres**

- Roguelite
- Doomer Shooter (?)
- FPS (?)


**Inspiration**

- Roboquest
	- Meta game upgrading system
		- Retaining runes between runs
	- Affixes / modifiers / weapon system
- Mothergunship
	- Weapon losing / inventory system
- Into the pit
	- Biome adversity
	- NPC development
- Neurovoider
	- Modularity
	- Varied enemies
	- Dynamic / procedural levels
- Dark Alliance
	- Hidden orbs / runes acquired ingame which are then revealed afterwards
	- Would require a specific kind of gameplay loop


**Open Discussion**

How will gameplay loop work:
- Roguelite runs, in which you build up a construct through single runs
	- Short sessions in and out
		- doesnt allow for much ingame development of larger constructs but could benefit meta progression
	- Longer sessions between multiple floors / rooms
		- bigger progression of construct but more lost inbetween each run
- Open world, going dungeon to dungeon / zone to zone building up 1 singular construct throughout entire playtime
	- Incorporate mothergunship style weapon system, of taking them into missions but losing them if you die
	- Requires more thought to force variety in builds / allow for meaningfull upgrades

Potential includable meta-game:
- NPC / shops in a hub area:
	- Buy runes / orbs to permanently keep / 1 time use
	- Unlock new runes orbs to be found ingame
- Perks to provide affinity towards certain tags?
- Shooting / testing range

---

## Scope Creep Central

- Totem that powers up rune and attacks you and you get it

- Random Affixes / Modifers for orbs / runes generated / found in the world

- Certain runes capable of levelling up through use, and becoming stronger
	- This could lead into skill tree / combinations / synergies with levelled up runes

- Multi-rune combinations performing different actions based on runes / positioning
	- E.g. a rune on an orbiting orb will react different to it on a base orb, or on an orb attached to an elbow construct orb

- Rune / Orb caches which can be broken and drop large amounts of runes / caches of different varieties
	- The caches themselves could come with specific rarities
	- Could link well into inter-game progress / meta game
	- Similar to dark alliance hidden things could get after runs rather than during

---
