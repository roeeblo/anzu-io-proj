using System;
using UnityEngine;

public class Tree2Command : ICommand
{
	private readonly bool _flag;
	private Tree2 _tree2;

	public Tree2Command(bool flag)
	{
		_flag = flag;
	}

	public void Execute()
	{
		_tree2?.setActiveTree2(_flag);
	}
}
