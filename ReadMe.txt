Step 1: Setting up your CSV.  Use this format to set up your CSV:  

Name,Level,Traits,Effect,Attack,Defense,Condition,Subtype,Affinity,Rarity,Type

Subtype refers to Active, Passive, or Counter.  The program will pull the first letter for you so you can spell it out.
Type refers to Spell or Unit.
Leave the cell blank if the card does not have that attribute (though it won't break it if you put random data there.)
Affinity will default to Land.
If Name has a "," such as "Caige, Sword of the Throne" use "" around it in the CSV. ("Caige, Sword of the Throne",10,Dragon Human Warrior,,14,13,,,,3,Unit)

Step 2: Set up your Images Folder.

Save the file names to be the name of a card.  If the card has an illegal filename character in it, replace it with an empty character, not a " ".
Use PNG with an aspect ratio of 500x700.  You can use higher definition, but to avoid stretching, make sure to keep that ratio.  500x700 will be the output resolution of the card though.
You do not need an empty PNG for card without art.  The card will just have a blank white background.

Step 3: Set up an Output Folder.

Not too much here, just don't use the same folder for Images as you use for Output.  Otherwise it will overwrite your Images with the cards.

Step 4: Running the program.

Open the Unity Project, or unzip the "LatestBuild" folder.
Either paste your csv filepath into the first field or click the ... button to open the file browser.  Repeat for Images and Output.
Click Generate Individual Cards to create a unique file for each card.
Click Generate Sheets to create a 3x3 grids of cards.  This is mainly for printing. 
