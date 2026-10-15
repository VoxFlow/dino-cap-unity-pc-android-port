#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;
using DinoCap.PortV2;

namespace DinoCap.PortV2.Tests
{
    // User-approved QoL (2026-10-14): Back/Esc must close an open keyboard FIRST and must not also pause the game, whichever keyboard app is installed.
    public class DC1DebugTextInputRepro
    {
        [TearDown] public void Reset() { GUIUtility.keyboardControl = 0; }

        [Test] public void BackWithNoFocusedFieldIsNotConsumed()
        {
            GUIUtility.keyboardControl = 0;
            Assert.IsFalse(DC1DebugTextInput.ConsumeBack(), "With no field focused and no soft keyboard, Back must reach the pause handler.");
        }

        [Test] public void BackWithAFocusedFieldClosesItAndIsConsumed()
        {
            GUIUtility.keyboardControl = 12345;
            Assert.IsTrue(DC1DebugTextInput.ConsumeBack(), "A focused field takes the Back press.");
            Assert.AreEqual(0, GUIUtility.keyboardControl, "The field's focus is dropped, which closes the soft keyboard.");
            Assert.IsFalse(DC1DebugTextInput.ConsumeBack(), "The next Back press is no longer consumed (it can pause).");
        }
    }
}
#endif
