//-----------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::onAdd( %this )
{
	%this.initialize();
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::initialize(%this)
{		
	%this.setSceneGroup(Utility.getCollisionGroup("EnemyAttacks"));
	%this.setSceneLayer(4);
	%this.fixedAngle = true;
	
	%this.fresh = true;
	
	%this.driftSpeed = 8;	
	%this.lifeSpan = 0.6 * 1000;	//ms
	
	%this.sizeRatio = $pixelsToWorldUnits;
	%this.myWidth = 70 * %this.sizeRatio;
	%this.myHeight = 70 * %this.sizeRatio;
	
	%this.setupSprite();
	
    %this.createPolygonBoxCollisionShape(%this.myWidth, %this.myHeight);
    %this.setCollisionShapeIsSensor(0, true);
    %this.setCollisionGroups( Utility.getCollisionGroup("Player") SPC Utility.getCollisionGroup("PlayerBlock") );
	%this.setCollisionCallback(true);
	
    %this.setUpdateCallback(true);
	
	%this.mySchedule = schedule(%this.lifeSpan, 0, "EnemyBladeStrikeEffect::deleteThis", %this);
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::setupSprite( %this )
{
	%this.addSprite("0 0");
	%this.setSpriteAnimation("GameAssets:bladestrikeAnim", 0);
	%this.setSpriteSize(%this.myWidth, %this.myHeight);
	%this.setAngle(%this.strikeAngle + %this.owner.orientation);
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::onCollision(%this, %object, %collisionDetails)
{
	if(%this.fresh)
	{
		if(%object.getSceneGroup() ==  Utility.getCollisionGroup("Player"))
		{			
			
			%this.owner.owner.bladeDamage += %object.hit(%this.strikeDamage, %this.owner.owner);
			%this.fresh = false;
			%this.setSpriteBlendColor(1, 0, 0, 0.75);
		}
	}
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::onUpdate(%this)
{
	%this.setPosition(%this.owner.getWorldPosistion(%this.owner.getOrientatedOffset(%this.owner.bladeXoffset, %this.owner.bladeYoffset)));
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::deleteThis( %this )
{
	%this.safeDelete();
}

//-----------------------------------------------------------------------------

function EnemyBladeStrikeEffect::onRemove( %this )
{
}
