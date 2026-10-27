# The Lab — Game Brief

A complete description of *The Lab*, a room-scale VR escape room, written so another designer or AI model can help develop ideas, redesign the space, and create art without breaking what already works.

---

## 1. At a glance

| | |
|---|---|
| **Genre** | Single-room VR escape room, 10–15 minutes |
| **Platform** | **PC VR for the recording: PSVR2 through the official PC adapter, via SteamVR/OpenXR.** The design still respects a Pico-class budget, because the employer's device runs on Pico |
| **Engine** | Unity 6.3 LTS, Universal Render Pipeline (URP), XR Interaction Toolkit 3.3 |
| **Player** | Standing, room-scale, two tracked controllers (hand tracking via webcam exists as an optional input layer) |
| **Tone** | Light sci-fi, dry humor, grounded lab hardware |
| **Why it exists** | Portfolio piece for an XR interaction developer role at a startup whose medical measurement device mounts on a Pico headset. It has to show precise, varied, physically believable hand interactions, plus a virtual guide character, which the employer requires. |
| **Deliverable** | A gameplay video and the source repository |
| **Current visual state** | Grey-box: Unity primitives and flat colors. All mechanics work; the art and layout are placeholders and are meant to be replaced. |

**One-sentence pitch:** You wake up locked inside a research lab that sealed itself after an incident, and a sarcastic safety drone walks you through the safety checklist the lockdown system demands before it will let you out.

---

## 2. The story and the structure

The player is inside **Lab 7**, a research lab that sealed itself after an unexplained "incident". The power is off, the room is dark, and the main door is locked by an automatic **lockdown**. The only thing still working is **D-7**, the lab's floating safety drone, who mostly worries about the incident report.

**Why the door is locked — the spine of the design.** The lockdown system will not release the door until the lab is safe again. A terminal beside the door lists the safety criteria, and each one is a task in the room. This gives every puzzle the same, honest reason to exist: it is a real lab hazard that a real lab protocol would require you to clear.

The shape of the game is therefore:

1. **Power (forced first).** Without electricity there are no lights and no terminal. Replace the burnt fuse.
2. **The safety checklist (four tasks, any order).** Fire, air, equipment, hazardous materials. The player chooses what to tackle and can move between tasks freely, like the task list in *Among Us*. The terminal tracks progress.
3. **Exit (forced last).** With the checklist complete the lockdown lifts, the bolts unlock, and the manual door release can finally be pulled.

**Design principle (a hard requirement):** the whole room is one connected situation. Every task must be something a real lab would actually require before declaring a room safe, and must visibly change the room. Nothing should feel like a random toy on a wall. Any new idea, prop, label or line of dialogue must support this.

**The incident, told without exposition.** The centrifuge was loaded unbalanced. It shook itself apart, threw a tube, knocked sample vials across the room, and the electrical fault that followed burnt the fuse and started a small fire in the waste bin. Every task in the checklist is a consequence of the same accident, so the room explains itself. D-7 denies knowing anything about it.

---

## 3. The space (current grey-box layout)

- **Room:** 8 m wide (X), 10 m deep (Z), ceiling 3.5 m. Floor at Y = 0.
  - Walls are at X = ±4 and Z = ±5.
  - North is +Z, south is −Z, east is +X, west is −X.
- **Player start:** near the center-east, facing north.
- **Beyond the door:** a short 2 × 3 m corridor floor. The escape "outside" is still undesigned.

| Wall | What is there now |
|---|---|
| **North** | West side: a lab bench with the **centrifuge** and the loose fourth tube. East side: the **cold storage fridge** for the sample vials |
| **East** | The **fuse box** at chest height; the **office desk**, whose top drawer slides open and holds the spare fuse; the burning **waste bin** in front of the wall; further south a wall shelf with the **fire extinguisher** and one sample vial |
| **West** | **Coolant/nitrogen station**: valve wheel, pressure gauge, and the vent fan above them that starts turning when the purge runs |
| **South** | The **exit door** in the center, the two-handed **release lever** to its right, and the **lockdown terminal** to its left |
| **Ceiling** | Five fluorescent fixtures; the room lamp sits in the center one and is off until power is restored |
| **Set dressing** | A lab bench with a sink (microscope, laptop, glassware, tube rack, paperwork, mug), three lab stools — one knocked over beside the centrifuge with papers scattered around it — book shelving packed with journals, a rolling whiteboard by the exit, a monitor desk, conduit running along the walls to the fuse box and the gas station, crates, and signage (LAB 7, EXIT, BIOHAZARD, a safety poster). None of it is interactive |

**Art pass.** The grey-box primitives have been replaced by real models wherever one
could be sourced under CC0 or CC BY — see `CREDITS.md`, which the public repository
depends on. Everything else (mugs, glassware, whiteboard handwriting, signage) is still
built from primitives, and reads well enough at VR distance.

**Known layout problems.** These are the developer's own critique and the main thing to redesign:
- The dressing still does not say what the lab actually researched.
- The floor in the middle of the room is bare; the lab reads as tidier than a lockdown should look.
- Two of D-7's step-1 voice clips were recorded for the old cabinet and are unused; those lines need re-recording.

---

## 4. The tasks in detail

### Step 1 — Restore power (fuse box → drawers → fuse → main switch)

**Player sees:** a dark room lit only by red emergency lamps and the fire in the waste bin. A closed fuse box on the east wall, cables running up from it into the conduit. An office desk against the same wall, its drawers shut.

**Player does:** swings the fuse box door open and sees two cartridge fuses side by side — one clear, one with **blackened glass** — and a main switch that is down. Pulls the burnt fuse out, searches the desk drawers (all six open), finds a spare that matches the fuse that survived, pushes it into the empty holder, and throws the main switch up.

**Rules:**
- Power returns only when **every** holder has a good fuse **and** the switch is up. Raising the switch first, or putting the burnt fuse back, simply does nothing; the moment both conditions hold, the lights come on.
- A holder accepts any fuse of the right type — including the burnt one, because a socket cannot tell a dead fuse from a live one. The puzzle, not the socket, checks whether it works.
- Two **decoy fuses** sit in other drawers: the same kind of cartridge, but one too small and one too large. The holders refuse them, the way a fuse of the wrong size would not fit a real holder. This is the intended mistake: the player has to compare against the good fuse still in the box.
- Power is one-way: once the lights are on, both fuses lock into their holders and the switch stays up.

**Result:** the emergency lamps cut out, the ceiling fixtures flicker on one by one, and the lockdown terminal boots with the checklist showing four red rows.

**Interactions shown:** a hinged door, constrained linear motion (drawers), carrying an object across the room, a keyed socket that refuses the wrong part, and a lever throw.


### The safety checklist — four tasks, any order

The terminal by the door lists them. Each one solved turns its row green.

**The checklist is live, not a record.** A solved task keeps watching its own condition: turn the valve out of the green band, pull a tube out of the balanced centrifuge or open the fridge door, and that row goes red again. If the whole list had already gone green, the lockdown re-engages: the terminal returns to DOOR SEALED and the lever loses power until the list is green again. Two steps are one-way on purpose: **power** (the good fuse locks into its socket the moment the lights return, because pulling it would have to black out the room and close the whole list) and **fire** (a fire that is out does not relight). Once the door is open the game is over and nothing re-seals it.

**The player can read the list from anywhere.** Pressing the primary button on either controller (X/A), or `3` on the keyboard in the simulator, brings up a floating panel about half a metre in front of the eyes showing exactly what the terminal shows; pressing again dismisses it. The panel is a remote feed of the terminal, not a second list, so there is only one place in the code that renders the checklist — and before power is restored it reads `NO SIGNAL`, because the terminal is dead.

#### Task A — Fire (extinguisher)

**Player sees:** flames in a metal waste bin below the fuse box, lighting the dark room before power is even restored. A fire extinguisher sits on a wall shelf.

**Player does:** picks up the extinguisher, points it at the flames and holds the trigger (XRI *activate*). The spray is a cone-shaped cast from the nozzle; the controller vibrates while spraying.

**Rules:**
- The fire needs about 2.5 seconds of continuous spray. Stop early and it grows back — partial effort does not count.
- It can be put out before power is restored; the checklist simply marks it done when it opens.

**Result:** the flames and their glow die out.

**Interactions shown:** grab plus trigger-activate on a held tool, aiming, sustained effort, haptics.

#### Task B — Air (nitrogen purge)

**Player sees:** an industrial exhaust station on the west wall, labelled **N2 EXHAUST — HOLD NEEDLE IN GREEN**: a square wall fan at the top, a real 0–1.6 MPa pressure gauge in the middle with a green band painted across its scale (60–70%) next to its printed red zone, and a red gate-valve handwheel at the bottom, fed by a pipe that runs down beside the fan. In the story the fire-suppression system flooded the room with nitrogen, and this is how it gets pumped back out.

**Player does:** turns the valve wheel by grabbing its rim and moving the hand around its center — up to 1.5 full turns from closed to open.

**Mechanics (this is what makes it "sensitive"):**
- The valve sets the *target* pressure; the actual pressure follows it like a damped spring, so **turning fast makes the needle overshoot**.
- Crossing the red line trips an **alarm**: the status light blinks, the pressure vents to zero, and the valve does nothing until the player closes it fully and starts again.
- The needle has to stay in the green band **continuously for 2 seconds**.

**Result:** the vent fan spins up and the row goes green.

**Interactions shown:** continuous rotary control over a multi-turn range, live visual feedback, a failure state with recovery, fine motor control.

**Tuning values:** green band 0.6–0.7, red line 0.9, hold time 2 s, spring stiffness 20, damping 3.

#### Task C — Equipment (balance the centrifuge)

**Player sees:** a benchtop centrifuge (the "LS-6", built from primitives because no free centrifuge model had a licence the public repository allows) rattling against its own lid, its sloped control panel reading **IMBALANCE / OPEN LID**. Through the tinted lid window: an aluminium rotor with six numbered holes, each ringed in a colour that it shares only with the hole opposite it (1-4 green, 2-5 orange, 3-6 purple). Three tubes sit in holes 1, 2 and 3; a fourth lies on the bench where the machine threw it. A sticker on the lid shows the rule without words: **LOAD OPPOSITE PAIRS**, with a correct loading marked OK and a lopsided one marked NO.

**Player does:** lifts the lid (a hinge grabbed and swung like the valve is turned), rearranges the tubes so they sit in **opposite pairs**, drops the fourth tube in, and closes the lid.

**Rules:**
- All four tubes must be loaded, and the average position of the loaded tubes must sit on the spin axis — in practice, two opposite pairs.
- Closing the lid on an unbalanced rotor makes it rattle again and re-display IMBALANCE.
- With a correct load it locks the lid, spins up for a 3-second test run, spins down and reports **BALANCED**.

**Result:** the row goes green, and the rattling that has been nagging the player since the lights came on finally stops.

**Interactions shown:** a hinged lid, multiple keyed sockets, spatial reasoning about symmetry, machine state feedback on a diegetic display.

#### Task D — Hazardous samples (cold storage)

**Player sees:** the rack of hazardous samples that the centrifuge knocked over, lying on its side on the centrifuge bench in an olive-green puddle. Three short screw-cap cryovials — red, blue and yellow caps, a yellow biohazard band — lie in the spill, one of them on the floor under the bench in its own drip. They look nothing like the long, thin centrifuge tubes, so nobody tries to put them in the centrifuge. On the west wall hangs a yellow **SPILL KIT — BIOHAZARD RESPONSE** cabinet. Across the room, a medical fridge on a steel stand carries a door sign (SAMPLES / KEEP AT 4°C); inside, a tube rack has three holes ringed red, blue and yellow.

**Player does:** reaches for a vial and finds that a bare hand cannot pick it up — a biohazard is not handled by hand. Opens the spill kit, takes the **tongs**, and uses them: the trigger closes the jaws on a vial, releasing it drops the vial into whatever holds it next. Carries each vial across the room in the tongs, drops it into the hole of its own colour, and closes the fridge door.

**How the tongs work:** the vials sit on their own interaction layer (`Tongs`) that neither hand can reach. The tongs carry a small socket between the jaws on that layer, inactive until the trigger is squeezed; releasing the trigger deactivates it and XRI lets the vial go, straight into the rack's socket if it is above one. `Tongs.cs` only switches that socket and animates the arms. The tongs hang on a keyed hook in the cabinet and can be put back.

**Rules:** a slot accepts only its own color. The task counts only when all three are in **and** the door is shut; when it is done the light turns green. The door does not lock: opening it again breaks containment and turns the light red.

**Interactions shown:** tool-mediated grabbing (an instrument that picks things up instead of the hand), sorting by a visual key, several validated sockets, two hinged doors, a completion condition that spans multiple objects.

### Step 3 — Open the door (two-handed lever)

**Player sees:** the terminal switches to **LOCKDOWN LIFTED / MANUAL RELEASE ENABLED**, and the large lever right of the door becomes live.

**Player does:** grabs the wide handle with **both hands at once** and pulls it down through about 80°.

**Rules:**
- One hand does nothing; the design reason is real — two-hand controls exist on industrial machinery so that neither hand can be in the mechanism.
- Releasing mid-pull springs the lever back. At the bottom it locks.
- A hand already holding something cannot grab the lever, so the player must put things down.
- The lever is dead until the checklist is complete.

**Result:** the door swings open over 2 seconds and the player walks out.

**Interactions shown:** two-handed interaction, sustained effort, a spring-back failure state.

---

## 5. The guide: D-7

**What it is:** a small floating lab drone with a single glowing cyan "eye", four rotors and a dark body. It has no face and no arms, which is a running joke. The eye flashes red during the pressure alarm.

**Behavior:**
- Hovers at the player's right, about a metre and a half ahead, bobbing and turning to face them.
- When it explains or hints, it **flies to the relevant object** for a few seconds, then returns.
- Has no collision, so it never blocks the player or an interaction. Instead it casts a sphere from the player's head toward wherever it wants to hover, and stops short of whatever blocks it, so it cannot slide into a wall or a bench.
- Its eye is a dim short-range light plus an emissive material, not a lamp: a bright light that close to the face is painful in a headset.
- Speaks with a **subtitle above its body** that always faces the player, plus a **generated voice** (young male, slightly robotic).

**Personality:** sarcastic but always helpful. It mocks the lab, the scientists, physics and the player's rushing, but every line contains a real clue. It is slightly guilty about the incident.

**Dialogue logic (updated for parallel tasks):**
- **Opening** and a **step intro** for power, for the checklist as a whole, and for the exit lever. The four checklist tasks have **no intros** — four introductions at once would be a lecture.
- **Hints** are per task and escalate from gentle to direct. When several tasks are open, the coach hints on **whichever open task has received the fewest hints**, so every task gets its gentle nudge before any gets a blunt one. A hint arrives only after the coach has been silent for its idle time (40–60 s).
- **Reactions:** dedicated lines for the pressure alarm.
- **On each solve:** a short line, including for individual checklist tasks.
- **Ending:** a goodbye as the door opens.

**Voice status:** lines written for the original laser puzzle are gone. The lines for power, the valve hints, the lever and the ending still have their recorded clips; the new lines (checklist intro, fire, centrifuge, samples, per-task solves) are **text-only** until new clips are generated. Lines without a clip stay on screen based on their length, so the game is fully playable meanwhile.

---

## 6. Feedback and atmosphere (current)

- **Lighting:** the room starts in near-black ambient, lit by four dim red emergency lamps (one per wall) and the fire. When the good fuse goes in, the emergency lamps cut out, half a second of total darkness follows, and then the five ceiling fixtures switch on one at a time — nearest the fuse box first, as if power were running along the conduit — each flickering twice like a real fluorescent tube before it stays lit, while the ambient level rises. All of this lives in `LabLighting`; `FusePuzzle` only raises `onPowerRestored`. Status lights: gauge (red/green, blinking on alarm) and fridge (red/green). The fire has its own flickering point light.
- **Displays:** the lockdown terminal (checklist, rich-text colored rows) and the centrifuge's small status display.
- **Sound:** only D-7's voice so far. No interaction sounds, ambience or music yet — the biggest remaining gap.
- **Haptics:** the extinguisher pulses the controller while spraying. The valve and the lid do not yet.
- **Materials:** flat grey walls with a few accent colors: red valve and extinguisher, dark metal, cream gauge face, white appliances, colored sample caps.
- **World text:** one label per station ("N2 AIR PURGE", "FIRE EXTINGUISHER", "CENTRIFUGE", "COLD STORAGE - BIOHAZARD", "MANUAL DOOR RELEASE").

---

## 7. What can change freely, and what must stay

The mechanics are built so the **layout and art can be redesigned without rewriting code**.

**Free to change:**
- Where each station stands, the room size and the wall layout.
- Every model, material, prop and decoration.
- All dialogue text and voice clips, and every hint timing.
- Tuning values: valve difficulty, tube count, balance tolerance, spray time, lever angle.
- Adding props, story details, labels, sound and lighting.
- The order of the checklist rows, and which tasks are in it.

**Must be preserved so the interactions keep working:**
- **Drawer:** a single sliding axis with limits; the fuse rides inside it physically.
- **Fuse / socket:** a one-handed object and a slot that visibly accepts only it.
- **Valve:** a round wheel grabbable at the rim, rotating on one axis, plus a readable gauge with a target band and a danger zone.
- **Extinguisher:** a grabbable body with a nozzle transform whose forward direction is the spray direction, and a fire that can be approached from a clear angle.
- **Centrifuge:** a rotor with slots at **equal angles and equal radius** (the balance check is geometric), a lid that rotates on one axis, and one more tube than the loaded ones.
- **Cold storage:** color-keyed slots and a door that rotates on one axis.
- **Lever:** a handle wide enough for two hands (~60 cm) with room to stand in front of it.
- **Door:** it sits in a real opening cut into the wall, so moving it means re-cutting the opening.
- **Guide drone:** free air space around the player and near each point of interest.

**Hard technical constraints for any art or layout ideas:**
- **PC VR (PSVR2 over SteamVR) is what will be recorded**, so real-time lights, shadows and modest transparency are affordable. Keep models low-poly and the texture budget small anyway, so the project can honestly claim it was built to a Pico-class budget.
- **XR setup:** OpenXR 1.18 with the OpenXR loader enabled for Windows and *Initialize XR on Startup* on; the Oculus Touch, Valve Index, HTC Vive and Khronos Simple controller profiles are enabled, because SteamVR remaps the PSVR2 Sense controllers onto one of them. The scene's `XR Interaction Simulator` object must be disabled when running on a real headset.
- **Real-world scale.** Interaction heights roughly 0.9–1.7 m; nothing the player must touch above ~1.8 m or on the floor.
- **Comfort.** No forced camera motion, no full-view flashing, enough clear floor to move.
- **Readability in VR.** Clear silhouettes, contrast, and interactables that look grabbable.

---

## 8. Open design questions (good places to ask for ideas)

1. **Art direction:** clean high-tech medical lab, retro-industrial research facility, or abandoned after the incident?
2. **Diegetic staging:** a real electrical panel instead of a lone box; pipes feeding the valve; a proper lab bench with equipment around the centrifuge; a fume hood; cable runs to the terminal.
3. **Room layout:** how to arrange the stations so the player naturally sweeps the whole room and always sees a next goal, now that four tasks are open at once.
4. **Sound design:** ambience that changes as systems come online (hum after power, fan after the purge, the centrifuge rattle stopping), interaction sounds (drawer, fuse click, valve squeak, extinguisher hiss, lid clunk, lever strain, bolts releasing), and the alarm.
5. **Moments of payoff:** how each solved task can transform the room visibly — monitors booting, steam, the fan, warning lights turning green, the bolts retracting.
6. **The ending:** what lies beyond the door, and whether D-7 gets a last beat (it cannot leave Lab 7).
7. **Optional additions** that fit the checklist without breaking it: a torn maintenance page about the valve, a rota sheet naming whoever loaded the centrifuge, a UV filter on a flashlight that reveals markings.

---

## 9. Technical summary (for model context)

- **Puzzle flow:** `PuzzleSequencer` runs a linear list of `PuzzleStep`s: power → safety checklist → exit lever. The checklist is a `PuzzleGroup`, itself a step, which activates its four tasks together and solves when all of them are solved. A step ignores the player until it is active, so nothing can be solved out of order — but the four tasks inside the group are all active at once.
- **Announcements:** `PuzzleStep` raises static `AnyActivated` / `AnySolved` events, so the coach and the terminal follow every step, including tasks nested in a group, without referencing any of them.
- **Loose coupling:** interactive objects announce events ("filled", "pulled", "extinguished", "alarm") or expose a value (valve position, dial angle). Puzzle controllers turn those into rules. Nothing references anything else directly, so any piece can be restyled or moved.
- **Custom interaction components** (on top of XR Interaction Toolkit):
  - Constrained slider (drawer).
  - Validated socket + socket key (fuse box, centrifuge slots, fridge slots). It announces **every** accepted insertion together with what went in, so a listener can decide whether that particular object counts — which is how the burnt fuse fits the socket without restoring power (`Fuse.IsBurnt`).
  - Constrained dial with optional click detents (valve, centrifuge lid, fridge door).
  - Two-handed lever.
  - Fire extinguisher (grab + activate + cone cast + haptics) and a fire that can be doused or regrow.
  - Spinner (vent fan, centrifuge rotor) and a door/hatch hinge.
- **Displays:** `LockdownTerminal` renders the checklist from the group's task list and their display names, and publishes that text; `ChecklistPanel` is a head-following panel that subscribes to it and toggles on an input action built in code (controller primary button, or `3`), so no Input Actions asset needs editing. The centrifuge writes its own status text.
- **The guide:** follow and point-of-interest movement, a speech queue with subtitles and audio, and a director that holds the per-step script and spreads hints across whatever tasks are open.
