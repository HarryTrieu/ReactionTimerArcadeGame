# A Simple Reaction Machine implemented in C# using a state machine architecture, featuring a console-based GUI, fully capable of:

+ Managing game states: Implements a finite state machine with Idle, Ready, Waiting, Running, Result, and Average states to control game flow.
+ Handling user inputs: Responds to Spacebar (coin insertion), Enter (go/stop action), and Escape (exit) key presses for user interaction.
+ Simulating a reaction time game: Measures player reaction times across up to three games per session, triggered by a random delay (1-2.5 seconds).
+ Displaying real-time feedback: Updates the console GUI with messages like "Insert coin," "Press GO!," "Wait...," reaction times (e.g., "0.00"), and average scores.
+ Tracking reaction times: Records reaction times for each game in an array, calculating and displaying the average after three games or timeout.
+ Detecting cheating: Flags premature Go/Stop presses during the waiting phase, ending the session if detected.
+ Enforcing timeouts: Transitions to Idle state after 10 seconds in Ready state, 2 seconds in Running state, or 3 seconds in Result state, and 5 seconds in Average state.
+ Generating random delays: Uses a custom random number generator (seeded for reproducibility) to set variable delays before the reaction prompt.
+ Rendering a console-based UI: Draws a bordered menu with instructions using ASCII characters, with color-coded text for visual clarity.
+ Maintaining game count: Tracks the number of games played (up to three) per coin insertion, resetting after each session.
+ Providing persistent state management: Resets game variables (ticks, reaction times, game count, cheated flag) appropriately when transitioning between states.
