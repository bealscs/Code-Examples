//-----------------------------------------------------------------------------
// Room setup
//  -Wall objects, player start, enemy locations
//  -Default first chromosome
//  -Text count down
//  -Background images
//  -Music
// Split room chromosome
// End room logic
//-----------------------------------------------------------------------------

function Arena::buildArena(%this)
{
    // Background
    %background = new Sprite() {class="backgroundObj";};
    %background.setBodyType( "static" );
    %background.setImage( "GameAssets:background" );
    %background.setSize( $roomWidth, $roomHeight );
    %background.setCollisionSuppress();
    %background.setActive( false );
    %background.setSceneLayer(30);
    %background.setSceneGroup( Utility.getCollisionGroup("") );
    %this.getScene().add( %background );
    
    // Arena Edges
    %roomEdges = new Sprite() {class="backgroundObj";};
    %roomEdges.setBodyType( "static" );
    %roomEdges.setImage( "GameAssets:backgroundedging" );
    %roomEdges.setSize( $roomWidth, $roomHeight );
    %roomEdges.setCollisionSuppress();
    %roomEdges.setActive( false );
    %roomEdges.setSceneLayer(30);
    %roomEdges.setSceneGroup( Utility.getCollisionGroup("") );
    %this.getScene().add( %roomEdges ); 
	
	%edgeWidth = 30 * $pixelsToWorldUnits;
    
	echo("edgeWidth " @ %edgeWidth);
	
    %this.addArenaBoundaries( %edgeWidth );
	
	echo("buildArena before music");
	%this.addBackgroundMusic();
	
    //%this.setUpdateCallback(true);
	
	%this.dropPickupChance = 15;
	
	//Populate room
	%this.player = %this.spawnPlayer(0, 0);		//add player before Enemies!
	
	//List of behaviors that pause
	%this.pausers = "ZigZagBehavior";
	//List of behaviors that get paused
	%this.pausees = "ChaseBehavior";
	%this.currentEnemies = new SimSet();
	
	echo("Before wave");
	%this.nextArenaWave();
	
}

//-----------------------------------------------------------------------------
// add boundaries on all sides of the Arena
function Arena::addArenaBoundaries(%this, %buffer)
{
    // Calculate a width and height to use for the bounds.
    %wrapWidth = $roomWidth * 1.0;
    %wrapHeight = $roomHeight * 1.0;	//1.1
	
	echo("buffer " @ %buffer);

    %this.getScene().add( %this.createOneArenaBoundary( "left",   -%wrapWidth/2 SPC 0,  %buffer SPC %wrapHeight) );
    %this.getScene().add( %this.createOneArenaBoundary( "right",  %wrapWidth/2 SPC 0,   %buffer SPC %wrapHeight) );
    %this.getScene().add( %this.createOneArenaBoundary( "top",    0 SPC -%wrapHeight/2, %wrapWidth SPC %buffer ) );
    %this.getScene().add( %this.createOneArenaBoundary( "bottom", 0 SPC %wrapHeight/2,  %wrapWidth SPC %buffer ) );
}

//-----------------------------------------------------------------------------
// create boundary at specified location
function Arena::createOneArenaBoundary(%this, %side, %position, %size)
{
    %boundary = new SceneObject() { class = "ArenaBoundary"; };
    
    %boundary.setSize( %size );
    %boundary.side = %side;
    %boundary.setPosition( %position );
    %boundary.setSceneLayer( 1 );
    %boundary.createPolygonBoxCollisionShape( %size );
	
    // the objects that collide with us should handle any callbacks.
    // remember to set those scene objects to collide with scene group 15 (which is our group)!
    %boundary.setSceneGroup( Utility.getCollisionGroup("Wall") );
    %boundary.setCollisionCallback(false);
    %boundary.setBodyType( "static" );
    return %boundary;
}

//-----------------------------------------------------------------------------
//start single background track on repeat
function Arena::addBackgroundMusic(%this)
{
	echo("arena: audio");		
	
	%musicAsset = "GameAssets:roomMusic";
	
	$roomMusicHandle = alxPlay(%musicAsset);	
	
	echo("Playing" @ alxIsPlaying($roomMusicHandle));
	
}

//-----------------------------------------------------------------------------

function Arena::nextArenaWave(%this)
{
	echo("In wave");
	%this.currentEnemies.clear();
	%this.roomChromosomes = "";
	
	// Enemy Info
	%this.EnemyCount = 0;
	%this.toolVarietyCount = 7;		//number of different tools available, length of local chromosomes
	
	//-RoomDamageTracking---
	//      (damage dealt by player)
	%this.roomShooterDamage = 0;
	%this.roomShooterShotsFired = 0;
	%this.roomBladeDamage = 0;
	%this.roomBladeAttackNums = 0;
	echo("Before font");
	%this.addRoomFont(-$roomWidth/2 + 1, $roomHeight/2 - 0.5);
	echo("Before join?");
	//$roomStartLag = 1;		//delay before READY text appears
	//%this.schedule($roomStartLag, "addReadyWarning", 0, 0); //TODO examine the font
	echo("Before joinn");
	echo("Before join");
	
	%this.schedule(1500, "spawning");
	
}

//-----------------------------------------------------------------------------

function Arena::updateGroup(%this )
{
	echo("updateGroup");
    for(%i = 0; %i < getWordCount(%this.currentEnemies); %i++)
    {
        // iterate the group, call each object's update() method
        %obj = %this.currentEnemies.getObject(%i);
        %obj.updateGroup();
    }
    // do it again in 250 ms
    schedule(32, "updateGroup", 0);
}

//-----------------------------------------------------------------------------
//handle spawning routines
function Arena::spawning(%this)
{
	%this.addReadyWarning(0,0);
	%this.schedule(1500, "addSteadyWarning", 0, 0);
	if(%this.currLevel != 1)
	{
		echo("grabbing choromosome");
		%this.currChromosome = $genAlg.retrieveChromosome();
		echo(%this.currChromosome);
	}
	%this.spawnEnemyPoof();
	//if(%this.currLevel == 1)
	//	%this.schedule(1500, "processRoomChromosomes");
	
	%this.schedule(3000, "addGoWarning", 0, 0);
	%this.schedule(3000, "processRoomChromosomes");
}

//-----------------------------------------------------------------------------
//display current level/room number
function Arena::addRoomFont(%this, %x, %y)
{
	%text = "Room:" @ %this.currLevel;
	//draw font 4 times (slightly offset) for pseudo boldness
	%this.drawText(%x, %y, "3 3", %text, "Left", "1 1 0");
	/*%this.drawText(%x + 0.1, %y, "3 3", %text, "Left", "1 1 0");
	%this.drawText(%x + 0.1, %y + 0.1, "3 3", %text, "Left", "1 1 0");
	%this.drawText(%x, %y + 0.1, "3 3", %text, "Left", "1 1 0");*/
}

//-----------------------------------------------------------------------------
//draw (posX, posY, size, text, alignment, color
function Arena::drawText(%this, %x, %y, %size, %text, %align, %colorBlend)
{
	%font = new ImageFont();
	%font.Image = "GameAssets:font";
	%font.Text = %text;
	%font.FontSize = %size;
	%font.setPosition(%x, %y);
	%font.TextAlignment = %align;
	%font.setBlendColor(%colorBlend);
	%this.getScene().add( %font ); 
	
	return %font;
}

//-----------------------------------------------------------------------------

function Arena::addReadyWarning(%this, %x, %y)
{
	%text = "READY?";
	%lifeSpan = 1500;
	
	//draw font 4 times (slightly offset) for pseudo boldness
	//TODO: READY/SET/GO 4 (each) font draws draws could be done through a function
	%this.drawText(%x, %y, "5 5", %text, "Center", "1 0 0").schedule(%lifeSpan, "safeDelete");	
}

//-----------------------------------------------------------------------------

function Arena::addSteadyWarning(%this, %x, %y)
{
	%text = "SET";
	%lifeSpan = 1500;
	//TODO: READY/SET/GO 4 (each) font draws draws could be done through a function
	%this.drawText(%x, %y, "6 6", %text, "Center", "1 1 0").schedule(%lifeSpan, "safeDelete");
}

//-----------------------------------------------------------------------------

function Arena::addGoWarning(%this, %x, %y)
{
	%text = "FIGHT!";
	%lifeSpan = 1500;
	//TODO: READY/SET/GO 4 (each) font draws draws could be done through a function
	%this.drawText(%x, %y, "7 7", %text, "Center", "0 1 0").schedule(%lifeSpan, "safeDelete");

}

//-----------------------------------------------------------------------------
//-spawn shadow dusts (effect for enemy spawn)
//TODO: should probably be moved to a separate function. Call from buildArena()
function Arena::spawnEnemyPoof( %this )
{
	%enemyUnitCount = mFloor(getWordCount(%this.currChromosome)/%this.toolVarietyCount);	//number of enemy units
	for(%i = 0; %i < %enemyUnitCount; %i++)
	{
		%this.spawnPoints[%i] = %this.findSpawnLocation();		//record a random spawn location (not on top of player)
	}
	
	%lifeSpan = 3000;
	//%enemyUnitCount = mFloor(getWordCount(%this.currChromosome)/%this.toolVarietyCount);
	for(%i = 0; %i < %enemyUnitCount; %i++)
	{
		%shadowDust = new CompositeSprite()			//create new dust object
		{
			class = "shadowDust";
			myArena = %this;
			lifeSpan = %lifespan;
		};
		
		
		
		%shadowDust.setPosition(%this.spawnPoints[%i]);	//move dust to corresponding enemy spawn loc.
		%this.getScene().add( %shadowDust );		//add dust object to scene
	}
}

//-----------------------------------------------------------------------------
// add a Player object to the Arena at specified location
function Arena::spawnPlayer(%this, %xPos, %yPos)
{
	%mainPlayer = new CompositeSprite()
	{
		class = "Player";
		myArena = %this;
	};
	
    %this.getScene().add( %mainPlayer );
	
	%mainPlayer.initialize();
	
	%mainPlayer.setPosition(%xPos, %yPos);

	return %mainPlayer;
} 

//-----------------------------------------------------------------------------
//break up Chromosome string and start spawning enemies according to sub-strings of the Chromosome
///ordering: armor/parry/acid/tar/blade/shooter/blob
function Arena::processRoomChromosomes(%this)
{
	echo("Chromosome:" SPC %chromosome);
	%chromosome = %this.currChromosome;
	
	for(%i = 0; %i < mFloor(getWordCount(%chromosome)/%this.toolVarietyCount); %i++)	//"for 0 through number of enemies"
	{
	
		%subChromosome = getWords(%chromosome, %i*%this.toolVarietyCount, (%this.toolVarietyCount - 1) + %i*%this.toolVarietyCount);	//get current group (7) of "words" (tool count/number)
		
		echo("  sub" SPC %subChromosome);
		
		echo("Arena.Arena: spawn enemy unit" SPC %i);

		%spawnLoc = %this.spawnPoints[%i];
		%this.spawnEnemyUnit(%subChromosome, getWord(%spawnLoc, 0), getWord(%spawnLoc, 1));	//spawn enemy at specified spawn location

		echo("Arena.Arena: spawned enemy unit successfuly" SPC %i);
		
		//collect subChromsomes back into a line-by-line list for post-room processing (no NewLine on last!!!)
		if(%i >= (getWordCount(%chromosome)/%this.toolVarietyCount) - 1)
		{
			%this.roomChromosomes = %this.roomChromosomes @ %subChromosome;
		}
		else
		{
			%this.roomChromosomes = %this.roomChromosomes @ %subChromosome NL " ";
		}
	}
	
	echo("Chromosome done processing!");
	%this.updateGroup();
}

//-----------------------------------------------------------------------------
//spawn enemy at specified spawn location

function Arena::spawnEnemyUnit(%this, %localChromosome, %xPos, %yPos)
{
	%newEnemy = new CompositeSprite()
	{
		class = "EnemyUnit";
		myChromosome = %localChromosome;
		myArena = %this;
		mainTarget = %this.player;
	};
	
    %this.getScene().add( %newEnemy );
	
	echo("Arena.Arena: initializing enemy:");
	%newEnemy.initialize();			//manual constructor function
	
	%newEnemy.setPosition(%xPos, %yPos);
	
	echo("currentEnemy " SPC )
	%this.currentEnemies.add(%newEnemy);

	return %newEnemy;
} 

//-----------------------------------------------------------------------------
//returns random location in Arena that leaves the middle clear for player start

function Arena::findSpawnLocation(%this)
{
	%position = "0 0";
	//$roomWidth/3), getRandom(-$roomHeight
	
	%zoneFrac = 5;
	%noSpawnZon_pointA = -$roomWidth/%zoneFrac SPC $roomHeight/%zoneFrac;
	%noSpawnZon_pointB = $roomWidth/%zoneFrac SPC -$roomHeight/%zoneFrac;
	
	%widthBorder = $roomWidth/8;
	%heightBorder = $roomHeight/8;
	
	%quadrant = getRandom(0, 4);
	
	if(%quadrant < 1)
	{
		%position = getRandom(-$roomWidth/2+%widthBorder, getWord(%noSpawnZon_pointB, 0)) SPC getRandom(getWord(%noSpawnZon_pointA, 1), $roomHeight/2 - %heightBorder);
	}
	else if(%quadrant < 2)
	{
		%position = getRandom(getWord(%noSpawnZon_pointB, 0), $roomWidth/2 - %widthBorder) SPC getRandom(getWord(%noSpawnZon_pointB, 1), $roomHeight/2 - %heightBorder);
	}
	else if(%quadrant < 3)
	{
		%position = getRandom(getWord(%noSpawnZon_pointA, 0), $roomWidth/2 - %widthBorder) SPC getRandom(-$roomHeight/2 + %heightBorder, getWord(%noSpawnZon_pointB, 1));
	}
	else
	{
		%position = getRandom(-$roomWidth/2 + %widthBorder, getWord(%noSpawnZon_pointA, 0)) SPC getRandom(-$roomHeight/2 + %heightBorder, getWord(%noSpawnZon_pointA, 1));
	}

	return %position;
} 

//-----------------------------------------------------------------------------
//tell Manager that the player died. Pass enemy chromosome who killed player to display on defeat screen

function Arena::playerDied(%this, %murderer )
{
	%this.myManager.playerDies(%murderer.myChromosome, %murderer.maxBodySize);
} 

//-----------------------------------------------------------------------------

function Arena::finishRoom(%this)
{
	%this.myManager.endCurrentLevel();
} 