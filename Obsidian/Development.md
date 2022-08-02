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
- [x] Refactor main components:
	- [x] Generic Construct class which handles the interactions between objects / runes
	- [x] PlayerController / PlayerCamera for better management of Forging mode as well as better interaction with Construct class
- [x] Implement a skill system that runes can subscribe skills to
- [x] Basic forging pause capability pass through to different Construct parts

### v0.3: Initial UX and UI
- [x] Better camera for ingame:
	- [x] Smoother movement with clearer positioning
	- [x] clipping / moving in front of objects
- [x] Develop a generic system for data for runes / orbs / cores
- [x] Initial Element data structure to handle keeping track of images, colors, names, and other config values
- [ ] Create a dynamic UI that dynamically shows data for:
	- [x] Objects
	- [x] Runes
	- [ ] Cores
- [ ] Expanding forging mode:
	- [x] Better checking of whether can enter forging
	- [x] Better position / rotation of construct and camera
	- [x] Highlight with UI on hover using the above system
	- [ ] Highlighting rune slots when available / free
- [x] Better feel for objects, involving weight / movement resist / movement calculation
- [ ] Clean up physics / movement bugs as well as tune values for gameplay feel

### v0.4: Initial Content
- [ ] Implement [[Objects#Destructible]] / damage system (for use with both objects as well as runes / enemies)
- [ ] [[Rune#Blaster]] weapon skill rune to test the destructable objects and skill system
- [ ] Energy system for use by skills from runes e.t.c that can deplete / regeneration
- [ ] Health for individual orbs / overall construct:
	- [ ] Abstract Damage class capable of broadcasting to other objects
	- [ ] Teams to allow for friendly / enemies to damage / not damage each other
- [ ] Initial meshes / materials:
	- [ ] Terrain generic standard grassy forest-like environment
	- [ ] Environment for rocky ruins with barrels / chests
- [ ] Visual and audio updates:
	- [ ] Simple grass shader with wind and movement out of the way
	- [ ] Add SFX for all common actions / interactions with constrtuct / menus
	- [ ] Update particle effects for MovementHop and add basic ones for MovementHover

### v0.5: Expanding UI
- [ ] Hotbar for skills including active / cooldown / usable
- [ ] Specific contextual crosshairs where skills can add to the crosshair on specific conditions and show keybinds / animations of possible actions
- [ ] Show visually whether in forging mode and keybinding
- [ ] Show health / energy visually for each construct part
	...

### Backburner
- [ ] Implement inverse kinematics for [[Rune#Crawl]] movement rune to test the Construct classes ability to manage Movement classes

- [ ] Better physics handling for:
	- [ ] Runes in rune slots
	- [ ] Cores while detaching
	- [ ] Movement hop with attack skill

---

## Short-term Todo

- [x] Set forging camera local z to preset zoom amount

- [x] Update ForgingState to use new IngameState logic for camera

- [x] Update calculated zoom range with new smoother camera system

- [ ] Change MovementHop AttackSkill particles size based on object

- [x] Sort out weights / moveResist etc with RigidBody and WorldObject

- [x] Capability of rotating around construct

### Ideas

- Construct Shape variable written to from RuneHandler  
  
- Pass certain bits to Shape such as SetForging etc

- Specifies focused construct object in forging mode  

- Can switch between objects and camera moves  

- Orb / Shape specific attachment, such as for 2-part top and bottom shape etc  
  
Orbs with preset shapes which the above accommodates for

---

## Bugs / Issues

- [x] Hop no longer hopping

- [x] Can forge during detachment

- [ ] MovementHover core jab can hit other objects
	- This issue morphed into that if the object rotates then you jab through it
	- Basically need to redo / bug fix the jab final positioning

- [ ] Clip through the floor on detachment of core

- [x] Movement hop not correctly calculating isGrounded

- [ ] Can highlight core while attaching

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
	- Hidden orbs / runes / loot acquired ingame which are then revealed afterwards
	- Would require a specific kind of gameplay loop
- Nimbatus
	- Can choose to make construct before entering the level
	- Similar to mothergunship of going into levels risk / reward of getting new stuff
- Sundered
	- Randomly, procedurally generated rooms in preset dungeon


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
