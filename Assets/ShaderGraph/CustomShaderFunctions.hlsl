#ifndef CUSTOMSHADERFUNCTIONS_INCLUDED
#define CUSTOMSHADERFUNCTIONS_INCLUDED

/// <summary>
/// Given 3 floats, determine the lowest
/// </summary>
/// <param name="Val1">First Value</param>
/// <param name="Val2">Second Value</param>
/// <param name="Val3">Third Value</param>
/// <param name="p_min">Out, min value of the three</param>
void TripleMin_float(float Val1, float Val2, float Val3, out float Min)
{
	Min = Val1 < Val2 ? (Val1 < Val3 ? Val1 : Val3 ) : (Val2 < Val3 ? Val2 : Val3 );

	/*
	Expanded version of above
	if(Val1< Val2)
	{
		if(Val1 < Val3)	
		{
			p_min = Val1;
		}
		else
		{
			p_min = Val3;
		}
	}
	else //  Val2 < Val1
	{
		if(Val2 < Val3)	
		{
			p_min = Val2;
		}
		else val1 < Val3
		{
			p_min = Val3;
		}
	}
	*/
}

/// <summary>
/// Given position and gridth width use Modulas to calc distance to nearest grid
/// </summary>
/// <param name="Position">Position of vert</param>
/// <param name="GridWidth">Distance betweem each grid</param>
/// <param name="Distance">Out, Distance from position to closest grid</param>
void DistanceToGrid_float(float Position, float GridWidth, out float Distance)
{
	Distance = Position % GridWidth;

	if(Distance < 0.0f) //Invert if negative
	{	
		Distance = -Distance;
	}

	float halfWidth = GridWidth/2.0f;

	if(Distance > halfWidth)
		Distance = GridWidth - Distance;
}

/// <summary>
/// Given position and grid width use Modulas to cal distance to nearest grid
/// </summary>
/// <param name="DistanceToGrid">Distance from point to the grid</param>
/// <param name="GridWidth">Width of grid line, This is total width, therefor a width of .2, will be full at a distance of .1, due to both sides</param>
/// <param name="GridBlend">Distance to blend line</param>
/// <param name="GridBlend">Out, Percent to grid line, 1.0 is on the line, 0.0-1.0 is the blend factor</param>
void GridPercent_float(float DistanceToGrid, float GridWidth, float GridBlend, out float GridPercent)
{
	float halfWidth = GridWidth/2.0f;

	if(DistanceToGrid <= halfWidth)
	{
		GridPercent = 1.0f;
	}
	else if(DistanceToGrid >= halfWidth + GridBlend)
	{
		GridPercent = 0.0f;
	}
	else
	{

//DG = .3
//GridW = .4
//blend = .2
//hGW = .2

//GP = .5

		GridPercent = 1.0f- ((DistanceToGrid - halfWidth) / GridBlend);
	}
}


#endif
