//-----------------------------------------------------------------------------
// This is the ToolNode parent class for all specific tool types
// Also acts as blob if not inherited from
//-----------------------------------------------------------------------------

function ToolNode::CreateInstance(%emyOwner, %type, %posX, %posY, %toolOrientation)  
{  
echo("Node Error Error Error Error!");
    %r = new SceneObject()  
	{  
		class = "ToolNode";  
		owner = %emyOwner;
		toolType = %type;
		bodyPosX = %posX;
		bodyPosY = %posY;
		orientation = %toolOrientation;		//direction relative to body that its facing
		drawSprite = true;
		stackLevel = 1;
	};  
  
    return %r;  
}  

//-----------------------------------------------------------------------------

function ToolNode::onAdd( %this )
{
}

//-----------------------------------------------------------------------------

function ToolNode::initialize(%this)
{	
	%this.setSceneGroup(Utility.getCollisionGroup("Enemies"));		//Enemy Unit sceneGroup

	%this.myWidth = 64 * %this.owner.sizeRatio;
	%this.myHeight = 64 * %this.owner.sizeRatio;

	%this.setupBehaviors();
	%this.setupCollisionShape();

	%this.setCollisionCallback(false);
	
	%this.setupSprite();
	
	if(%this.toolType $= %this.owner.blobToolName) 
	{
		%this.blobBonus();
	}
	
	%this.addStackSprites();
}

//-----------------------------------------------------------------------------

function ToolNode::setupCollisionShape( %this )
{
	%offsetX = %this.myWidth/2;
	%offsetY = %this.myHeight/2;
	
	%shapePoints = 
	%this.bodyPosX*%this.myWidth - %offsetX SPC %this.bodyPosY*%this.myHeight - %offsetY SPC 
	(%this.bodyPosX + 1)*%this.myWidth - %offsetX SPC %this.bodyPosY*%this.myHeight - %offsetY SPC 
	(%this.bodyPosX + 1)*%this.myWidth - %offsetX SPC (%this.bodyPosY + 1)*%this.myHeight - %offsetY SPC 
	%this.bodyPosX*%this.myWidth - %offsetX SPC (%this.bodyPosY + 1)*%this.myHeight - %offsetY;
		
	%this.owner.createPolygonCollisionShape(%shapePoints);
}

//-----------------------------------------------------------------------------

function ToolNode::setupSprite( %this )
{
	if(%this.drawSprite)
	{
		
		%this.owner.addSprite(%this.bodyPosX*%this.myWidth SPC %this.bodyPosY*%this.myHeight);
		
		if(%this.toolType $= %this.owner.blobToolName) 
		{
			%this.setupSpriteBlob();
		}

		%this.owner.setSpriteAngle(%this.orientation);
		
	}
}

//-----------------------------------------------------------------------------

function ToolNode::addStackSprites( %this )
{
	%perColumn = 4;
	
	for(%i = 0; %i < %this.stackLevel - 1; %i++)
	{
		%x = %this.bodyPosX*%this.myWidth;
		%y = %this.bodyPosY*%this.myHeight;
		
		%offsetter = 2.5;
		%increment = 13 * %this.owner.sizeRatio;
		
		if(%this.orientation == 0)
		{
			%y = %y - ((%this.myHeight/%offsetter) + (mFloor(%i/%perColumn)*1.25));
			
			%x = %x + %increment*(%i % %perColumn);
		}
		else if(%this.orientation == 90)
		{
			%x = %x - ((%this.myWidth/%offsetter) + (mFloor(%i/%perColumn)*1.25));
			
			%y = %y + %increment*(%i % %perColumn);
		}
		else if(%this.orientation == 180)
		{
			%y = %y + ((%this.myHeight/%offsetter) + (mFloor(%i/%perColumn)*1.25));
			
			%x = %x - %increment*(%i % %perColumn);
		}
		else if(%this.orientation == 270)
		{
			%x = %x + ((%this.myWidth/%offsetter) + (mFloor(%i/%perColumn)*1.25));
			
			%y = %y - %increment*(%i % %perColumn);
		}
		
		%this.owner.addSprite(%x SPC %y);
		
		%this.owner.setSpriteImage("GameAssets:tool_toolStacking", 0);
		%this.owner.setSpriteSize(12 * %this.owner.sizeRatio, 12 * %this.owner.sizeRatio);
		
		%this.owner.setSpriteAngle(%this.orientation);
	}
}

//-----------------------------------------------------------------------------

function ToolNode::setupSpriteBlob( %this )
{
	%this.owner.setSpriteImage("GameAssets:tool_blob1x1_b", 0);
	%this.owner.setSpriteSize(88 * %this.owner.sizeRatio, 88 * %this.owner.sizeRatio);
	%this.sortLevel = 2;
}

//-----------------------------------------------------------------------------
// Health bonus for blob tool types

function ToolNode::blobBonus( %this )
{
	%healthBonus = 10 * %this.stackLevel;
	
	//add to unit's total health
	%this.owner.fullHealth = %this.owner.fullHealth + %healthBonus;
	%this.owner.health = %this.owner.fullHealth;
}

//-----------------------------------------------------------------------------

function ToolNode::setupBehaviors( %this )
{

}

//-----------------------------------------------------------------------------

function ToolNode::getAdjacentSlots( %this )
{
	if(%this.toolType !$= %this.owner.blobToolName)
		return "";
		
	%right = (%this.bodyPosX + 1) SPC (%this.bodyPosY);
	%up = (%this.bodyPosX) SPC (%this.bodyPosY + 1);
	%left = (%this.bodyPosX - 1) SPC (%this.bodyPosY);
	%down = (%this.bodyPosX) SPC (%this.bodyPosY - 1);
	
	return %right SPC %up SPC %left SPC %down @ " ";
}
//-----------------------------------------------------------------------------
//get position in the body  grid of EnemyUnit

function ToolNode::getBodyPosistion( %this )
{
	return (%this.bodyPosX) SPC (%this.bodyPosY);
}

//-----------------------------------------------------------------------------
//get (x, y) position of the tool relative to the center of the body

function ToolNode::getRelativePosistion( %this )
{
	return (%this.bodyPosX*%this.myWidth) SPC (%this.bodyPosY*%this.myHeight);
}

//-----------------------------------------------------------------------------
//get the exact (x, y) of the tool in the frame of the world, not related to the rest of body

function ToolNode::getWorldPosistion( %this )
{
	return %this.owner.getWorldPoint(%this.getRelativePosistion());
}

//-----------------------------------------------------------------------------
//get the exact (x, y) of the tool in the frame of the world, with (relative) offset

function ToolNode::getWorldPosistion( %this, %xOffset, %yOffset )
{
	%pos = %this.getRelativePosistion();
	%x = getWord(%pos, 0) + %xOffset;
	%y = getWord(%pos, 1) + %yOffset;

	return %this.owner.getWorldPoint(%x SPC %y);
}

//-----------------------------------------------------------------------------
//Enter offsets as if tool is not rotated, returns rotated values according to orientation

function ToolNode::getOrientatedOffset(%this, %xOffset, %yOffset)
{
	%x = 0;
	%y = 0;
	
	if(%this.orientation == 0)
	{
		%x = %xOffset;
		%y = %yOffset;
	}
	else if(%this.orientation == 90)
	{
		%x = %x - %yOffset;
		%y = %y + %xOffset;
	}
	else if(%this.orientation == 180)
	{
		%x = %x - %xOffset;
		%y = %y - %yOffset;
	}
	else if(%this.orientation == 270)
	{
		%x = %x + %yOffset;
		%y = %y - %xOffset;
	}
	
	return %x SPC %y;
}

//-----------------------------------------------------------------------------

function ToolNode::destroy( %this )
{
}
