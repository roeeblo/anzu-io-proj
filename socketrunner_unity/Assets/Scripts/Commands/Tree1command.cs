using System;
using UnityEngine;

public class Tree1Command : ICommand
{
	private readonly bool _flag;
	private Tree1 _tree1;

	public Tree1Command(bool flag)
	{
		_flag = flag;
	}

	public void Execute()
	{
		_tree1?.setActiveTree1(_flag);
	}
}
