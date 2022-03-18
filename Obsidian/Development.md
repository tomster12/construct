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
- [ ] Basic forging screen to allow better rune slotting / unslotting

### v0.3: Initial UX
- [ ] Add a generic / expandable system for object information for runes / orbs / cores
- [ ] Create a UI that dynamically shows information for hovered world objects - This will change and work correctly for each type of object information data
- [ ] Better camera scripts for clipping / moving in front of objects
- [ ] Expanding the interface for during forging:
	- [ ] Expanding tooltip / box on hovering
	- [ ] Highlighting rune slots when required

### v0.4: Initial Content
- [ ] Implement destructable objects / damage system (for use with both objects as well as runes / enemies)
- [ ] Implement a skill system that runes can subscribe skills to
- [ ] [[Rune#Blaster]] weapon skill rune to test the destructable objects and skill system
- [ ] Basic health management for overall construct / individual orbs

### v0.5: Expanding UI
- [ ] Dynamic UI for ingame:
	- [ ] Hotbar for skills
	- [ ] Resizing crosshair on hovering
		- This could include a unique animation for expanding around hovered objects
- [ ] Some visual indicator of health for construct / orb onscreen

### Backburner
- [ ] Implement inverse kinematics for [[Rune#Crawl]] movement rune to test the Construct classes ability to manage Movement classes

---

## Short-term Todo

- [x] Create PlayerController script which handles enabling / disabling the ConstructController / ConstructCamera, and the ForgingController / ForgingCamera script based on a GameState variable

- [x] Change both camera scripts to use a prevAimedWJ and to unhighlight in the Controller, to allow for more specific control

- [x] Seperate out effects on the camera so different scripts can easily access

- [x] Change attachment to be based on movement

- [x] Add in basic meshes for developing / vibe

- [x] Update MovementHop to have:

	- [x] jumpCooldown stat
	
	- [x] Jump to scale better with moveResist

- [x] Refactor to use Construct class which deals with runes / cores / orbs

	- [x] Reimplement Forging Camera / Controller

- [ ] Add UI for objects information for orbs / runes / cores
	
	- [ ] This UI can change depending on the type of objects hovered

- [ ] Deal with physics better on hover attachment / detachment

- [ ] Create a ForgingController / ForgingCamera script which accesses ConstructController to correctly position and access current [[Core|core]] / [[Orb|orb]] / [[Rune|rune]] slots and positions during [[Index#Forging|forging]] mode

	- [x] Hide UI on Forging toggle
	
	- [ ] Potentially use a function which gets a structured / unstructured list of all world objects in the current construct
	
	- [ ] Highlight objects from current construct when hovering
	
	- [ ] SFX For entering / exiting Forging

	
	> Ensure the [[Index#Forging|forging]] scripts are decoupled from the scripts on the individual world objects handled by the [[Construct]] script to ensure future proofing for more complex shapes

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


**Open Discussion**

How will gameplay loop work:
- Roguelite runs, in which you build up a construct through single runs
	- Short sessions in and out - doesnt allow for much ingame development of larger constructs but could benefit meta progression
	- Longer sessions between multiple floors / rooms - bigger progression of construct but more lost inbetween each run
- Open world, going dungeon to dungeon / zone to zone building up 1 singular construct throughout entire playtime
	- Incorporate mothergunship style weapon system, of taking them into missions but losing them if you die

What meta game is needed:
- NPC / shops in a hub area:
	- Buy runes / orbs to permanently keep / 1 time use
	- Unlock new runes orbs to be found ingame
- Perks to provide affinity towards certain tags?
- Shooting / testing range

---

## Scope Creep Central

- Random Affixes / Modifers for orbs / runes generated / found in the world
- Certain runes capable of levelling up through use, and becoming stronger
	- This could lead into skill tree / combinations / synergies with levelled up runes
- Multi-rune combinations performing different actions based on runes / positioning
	- E.g. a rune on an orbitting orb will react different to it on a base orb, or on an orb attached to an elbow construct orb
- Rune / Orb caches which can be broken and drop large amounts of runes / caches of different varieties
	- The caches themselves could come with specific rarities

---

## Architecture

### Player

**PlayerController**: Updates correct *Controller*, swaps between gamestate.

> **PlayerForgingController**: Update object positions.
> 
> - **PlayerForgingCamera**: Position camera for Forging.

> **PlayerConstructController**: Send movement to the *Construct*.
> 
>  - **PlayerConstructCamera**: Follow / rotate around the *Construct*.


<br />

### World

**WorldObject**: Allows easy access to mesh, bounds, outline, rigidbody


**Construct**: Manages core, orbs, runes, movement, aiming, attachment

> **Rune Handler**: Slots *Runes*
> 
> - **Rune**: Holds functionality / stats
> 
>   - **Rune Blank**: *Implementation*


> **MovementI**: Movement / aiming and setting active / inactive
>
> - **MovementHop**: *Implementation*
>
> - **CoreMovementI**: Attach / detach requirement
>
>   - **MovementHover**: *Implementation*


<br />

### Utility

**CameraEffects**: Utility for applying effects to Camera

**Easing**: Static mathematical functions

**StatList**: Serialized dictionary of stats / affectors

---
