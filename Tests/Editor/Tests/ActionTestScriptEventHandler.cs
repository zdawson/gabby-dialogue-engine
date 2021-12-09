using GabbyDialogue;
using NUnit.Framework;

public class ActionTestScriptEventHandler : AbstractScriptEventHandler
{
    public string actionCalled;

    [ActionHandler]
    private void TestSimple()
    {
        actionCalled = nameof(TestSimple);
    }

    [ActionHandler("TestActionWithDifferentNameThanHandler")]
    private void DifferentName()
    {
        actionCalled = nameof(DifferentName);
    }

    [ActionHandler]
    private void TestString(string s)
    {
        actionCalled = nameof(TestString);
        Assert.AreEqual(s, "Test");
    }

    [ActionHandler]
    private void TestBool(bool b)
    {
        actionCalled = nameof(TestBool);
        Assert.AreEqual(b, true);
    }

    [ActionHandler]
    private void TestInt(int i)
    {
        actionCalled = nameof(TestInt);
        Assert.AreEqual(i, 1);
    }

    [ActionHandler]
    private void TestUint(uint u)
    {
        actionCalled = nameof(TestUint);
        Assert.AreEqual(u, 1u);
    }

    [ActionHandler]
    private void TestFloat(float f)
    {
        actionCalled = nameof(TestFloat);
        Assert.AreEqual(f, 1.0f, float.Epsilon);
    }

    [ActionHandler]
    private void TestDouble(double d)
    {
        actionCalled = nameof(TestDouble);
        Assert.AreEqual(d, 1.0, double.Epsilon);
    }

    [ActionHandler]
    private void TestMultipleParameters(string s, int i, float f, bool b = true)
    {
        actionCalled = nameof(TestMultipleParameters);
        Assert.AreEqual(s, "Test");
        Assert.AreEqual(i, 1);
        Assert.AreEqual(f, 1.0f, float.Epsilon);
        Assert.AreEqual(b, true);
    }

    [ActionHandler]
    private void TestDefaultParameters(string s = "Default")
    {
        actionCalled = nameof(TestDefaultParameters);
        if (s != "Default" && s != "Test")
        {
            Assert.Fail();
        }
    }
}

public class ActionHandlerTest : AbstractScriptEventHandler
{
    [ActionHandler]
    private int returnTypeIsNotVoid()
    {
        return 0;
    }
}
