//-----------------------------------------------------------------------------
// Basic blade AI
//-----------------------------------------------------------------------------

if (!isObject(BladeToolBehavior))
{
    %template = new BehaviorTemplate(BladeToolBehavior);

    %template.friendlyName = "Blade tool AI";
    %template.behaviorType = "AI";
    %template.description  = "Repeat attack if in range";
}

function BladeToolBehavior::onBehaviorAdd(%this)
{
	%this.mySchedule = schedule(getRandom(%this.owner.cooldownTime), 0, "BladeToolBehavior::doAttack", %this);
	%this.attackRange = 124*%this.owner.owner.sizeRatio;
}

function BladeToolBehavior::doAttack(%this)
{
	if (isObject(%this.owner.owner) && isObject(%this.owner.owner.mainTarget))
	{
		%targDist = Vector2Distance(%this.owner.owner.mainTarget.getPosition(), %this.owner.getWorldPosistion());
		
		if(	%targDist <= %this.attackRange)
		{
			%this.owner.attack();
			%this.mySchedule = schedule(%this.owner.cooldownTime, 0, "BladeToolBehavior::doAttack", %this);
		}
		else
		{	
			%this.mySchedule = schedule(500, 0, "BladeToolBehavior::doAttack", %this);
		}
	}
}

function BladeToolBehavior::onBehaviorRemove(%this)
{
}
