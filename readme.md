named originally: unity_developer_junior__middle__1Zi017l5.md  

# Unity Developer Junior+/Middle+

Goal
====

![](https://t4554831.p.clickup-attachments.com/t4554831/381c2803-a160-48f4-a1e8-ceaa8df67154/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)

_Main card referenc_e

  

Create UI for an "in-hand card" object for CCG-like game. Card consist of:

*   Art + UI overlay
*   Title
*   Description
*   Attack icon + text value
*   HP icon + text value
*   Mana icon + text value

Load card art randomly from [https://picsum.photos/](https://picsum.photos/) each time app starts.

Fill player’s hand with 4-6 cards in a visually pleasing way and use the arc pattern for displaying the cards (look at the pic below). The number of cards should be determined randomly at the start of the game.

![](https://t4554831.p.clickup-attachments.com/t4554831/cf4e65c4-e494-41d5-a7fc-6d3ff062aba9/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)

_arc example_

Create an UI button at the center of the screen to randomly change one randomly selected value -2→9 (the range is from -2 to 9) of each one card sequentially, starting from the most left card in the player's hand moving right and repeating the sequence after reaching the most right card.

Bind Attack, Health and mana properties to UI. Changing those values from code must be reflected on the card's UI with counter animation. (counting from the initial to the new value)

If some card’s HP drop below 1 - remove this card from player’s hand. (dont forget to reposition other cards, use tweens to make it smooth)

**\[Middle+\]** Player can drag a card and drop it on middle section of the table (use drop panel of any size) Card moves back to player’s hand if it’s hasn't been dropped over the drop panel. Cards shines when its being dragged.

Reference: Hearthstone, MTG Arena, Gwent and other same games.

—>

Upload results to GitHub and send the link to [hr@gamesredrift.com](mailto:hr@gamesredrift.com) with **_\[Unity Test\] <Your Name>_** in title, or directly to the representative.

<—

Treat this task as a real game project scenario. Even if a certain feature or requirement wasn't described clearly and in detail - it doesn't mean you can leave it raw and unfinished.

🧀 What's important
-------------------

* * *

1.  Project folder & hierarchy structure.
2.  Code style, naming, clearness. Proper use of Unity's api. SOLID
3.  Logic. Attention to details.
4.  UI layout
5.  The simplicity and conciseness of the solution is a must.
6.  Use of common practices, Optimization.
7.  Tween usage.
8.  Test completion speed. Overcomplicating solution is a failure.

🛑 What's not important
-----------------------

* * *

1.  Visuals used
2.  Any non goal related objects or functionality

* * *

💪 Good luck!

Use your own art (or unity ui) to perform this task or grab it below:

![](https://t4554831.p.clickup-attachments.com/t4554831/357f45d8-f65c-4e66-a2bb-0446fefe9c0b/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)![](https://t4554831.p.clickup-attachments.com/t4554831/eaeeacc8-cf8b-4bb4-9240-85f124c2322d/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)![](https://t4554831.p.clickup-attachments.com/t4554831/83ec9c00-b467-4224-bf1a-0ab249598e4b/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)![](https://t4554831.p.clickup-attachments.com/t4554831/215fd988-0323-4bb2-a73e-20643868eca1/%D0%B8%D0%B7%D0%BE%D0%B1%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5.png)