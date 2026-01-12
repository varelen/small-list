// SPDX-FileCopyrightText: 2026 varelen
// 
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

using Varelen.SmallList;

namespace SmallList.Tests;

public class SmallListTests
{
    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 10)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 36)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 55)]
    public void ForEach_DirectType_MatchesExpectedSum(int[] expectedArray, int expectedSum)
    {
        // Arrange
        var smallList = new SmallList<int>(expectedArray);

        int sum = 0;

        // Act
        foreach (int item in smallList)
        {
            sum += item;
        }

        // Assert
        Assert.Equal(expectedSum, sum);
    }

    [Theory]
    [InlineData(new int[] { }, 0)]
    [InlineData(new int[] { 1 }, 1)]
    [InlineData(new int[] { 1, 2 }, 3)]
    [InlineData(new int[] { 1, 2, 3}, 6)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 10)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 36)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 55)]
    public void ForEach_IEnumerable_MatchesExpectedSum(int[] expectedArray, int expectedSum)
    {
        // Arrange
        var smallList = new SmallList<int>(expectedArray);

        // Act
        int sum = Sum(smallList);

        // Assert
        Assert.Equal(expectedSum, sum);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static int Sum(IEnumerable<int> enumerable)
        {
            int sum = 0;
            foreach (int item in enumerable)
            {
                sum += item;
            }
            return sum;
        }
    }

    [Theory]
    [InlineData(new int[] { 100, 200, 300 }, -1)]
    [InlineData(new int[] { 100, 200, 300, }, 1)]
    [InlineData(new int[] { 100, 200, 300, }, 2)]
    [InlineData(new int[] { 100, 200, 300, }, 3)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, -1)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 1)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 2)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 3)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 4)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 5)]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600 }, 6)]
    public void CopyTo_InvalidArrayIndex_ThrowsIndexOutOfRangeException(int[] expectedArray, int arrayIndex)
    {
        // Arrange
        var smallList = new SmallList<int>(expectedArray);

        var actualArray = new int[expectedArray.Length];

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => smallList.CopyTo(actualArray, arrayIndex));
    }

    [Theory]
    [InlineData(new int[] { 100, 200, 300 })]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600, 700, 800 })]
    [InlineData(new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 })]
    public void CopyTo_GivenArray_CorrectlyCopies(int[] expectedArray)
    {
        // Arrange
        var smallList = new SmallList<int>(expectedArray);

        var actualArray = new int[expectedArray.Length];

        // Act
        smallList.CopyTo(actualArray, 0);

        // Assert
        for (int i = 0; i < expectedArray.Length; i++)
        {
            Assert.Equal(expectedArray[i], actualArray[i]);
        }
    }

    [Fact]
    public void Constructor_ZeroToFourArguments_MacthesExpectedItems()
    {
        // Arrange & Act
        var empty = new SmallList<int>();
        var emptyArray = new SmallList<int>([]);
        var emptyIEnumerable = new SmallList<int>(Enumerable.Empty<int>());
        var one = new SmallList<int>(1);
        var two = new SmallList<int>(1, 2);
        var three = new SmallList<int>(1, 2, 3);
        var four = new SmallList<int>(1, 2, 3, 4);
        var fourFromArray = new SmallList<int>([1, 2, 3, 4]);
        var fourFromIEnumerable = new SmallList<int>(Enumerable.Range(1, 4));

        // Assert
        AssertList(empty, []);
        AssertList(emptyArray, []);
        AssertList(emptyIEnumerable, []);
        AssertList(one, [1]);
        AssertList(two, [1, 2]);
        AssertList(three, [1, 2, 3]);
        AssertList(four, [1, 2, 3, 4]);
        AssertList(fourFromArray, [1, 2, 3, 4]);
        AssertList(fourFromIEnumerable, [1, 2, 3, 4]);
    }

    [Fact]
    public void Constructor_IEnumerableWhichIsAnICollection_MacthesExpectedItems()
    {
        // Arrange & Act
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var listWithInlineCount = new List<int> { 1, 2, 3, 4 };
        var empy = new SmallList<int>();
        var smallList = new SmallList<int>(list);
        var inlinedList = new SmallList<int>(listWithInlineCount);

        // Assert
        AssertList(empy, []);
        AssertList(smallList, [1, 2, 3, 4, 5, 6, 7, 8, 9]);
        AssertList(inlinedList, [1, 2, 3, 4]);
        Assert.Equal(16, smallList.Capacity);
    }

    [Fact]
    public void Add_MultipleItems_MatchesExpectedItems()
    {
        // Arrange
        var smallList = new SmallList<int>
        {
            // Act (calls 'Add')
            100,
            200,
            300
        };

        // Assert
        Assert.Equal(3, smallList.Count);
        Assert.Equal(100, smallList[0]);
        Assert.Equal(200, smallList[1]);
        Assert.Equal(300, smallList[2]);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 8)]
    [InlineData(9, 16)]
    public void Add_ManyItemsToForceGrowing_MatchesExpectedCapacity(int count, int expectedCapacity)
    {
        // Arrange
        var smallList = new SmallList<int>();

        // Act
        for (int i = 0; i < count; i++)
        {
            smallList.Add(i);
        }

        // Assert
        Assert.Equal(count, smallList.Count);
        Assert.Equal(expectedCapacity, smallList.Capacity);
    }

    [Fact]
    public void Remove_AllInlined_CorrectlyRemovesItems()
    {
        // Arrange
        var smallList = new SmallList<int>(100, 200, 300, 400);

        // Act
        bool removedExisting = smallList.Remove(200);
        bool removedNonExisting = smallList.Remove(200);

        // Assert
        Assert.Equal(3, smallList.Count);
        Assert.True(removedExisting);
        Assert.False(removedNonExisting);
        Assert.Equal(100, smallList[0]);
        Assert.Equal(300, smallList[1]);
        Assert.Equal(400, smallList[2]);
    }

    [Fact]
    public void Remove_MoreThanInlinedItemsCount_CorrectlyRemovesItems()
    {
        // Arrange
        var smallList = new SmallList<int>([100, 200, 300, 400, 500, 600, 700]);

        // Act
        bool removedExisting = smallList.Remove(600);
        bool removedNonExisting = smallList.Remove(1000);

        // Assert
        Assert.Equal(6, smallList.Count);
        Assert.True(removedExisting);
        Assert.False(removedNonExisting);
        Assert.Equal(100, smallList[0]);
        Assert.Equal(200, smallList[1]);
        Assert.Equal(300, smallList[2]);
        Assert.Equal(400, smallList[3]);
        Assert.Equal(500, smallList[4]);
        Assert.Equal(700, smallList[5]);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, -1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, -1)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 6)]
    public void Indexer_InvalidGetIndex_ThrowsIndexOutOfRangeException(int[] items, int index)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => smallList[index]);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, -1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, -1)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 6)]
    public void Indexer_InvalidSetIndex_ThrowsIndexOutOfRangeException(int[] items, int index)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => smallList[index] = 42);
    }

    [Fact]
    public void Indexer_MultipleAccessesAllInlined_ReturnsExpectedItems()
    {
        // Arrange
        var smallList = new SmallList<int>(100, 200, 300, 400);

        // Act
        var first = smallList[0];
        var second = smallList[1];
        var third = smallList[2];
        var fourth = smallList[3];

        smallList[1] = 1000;
        var secondAfter = smallList[1];

        // Assert
        Assert.Equal(4, smallList.Count);
        Assert.Equal(100, first);
        Assert.Equal(200, second);
        Assert.Equal(300, third);
        Assert.Equal(400, fourth);
        Assert.Equal(1000, secondAfter);
    }

    [Fact]
    public void Indexer_MultipleAccessesExceedingInlinedItemsCount_ReturnsExpectedItems()
    {
        // Arrange
        var smallList = new SmallList<int>([100, 200, 300, 400, 500, 600]);

        // Act
        var first = smallList[0];
        var second = smallList[1];
        var third = smallList[2];
        var fourth = smallList[3];
        var five = smallList[4];
        var six = smallList[5];

        smallList[5] = 1000;
        var sixAfter = smallList[5];

        // Assert
        Assert.Equal(6, smallList.Count);
        Assert.Equal(100, first);
        Assert.Equal(200, second);
        Assert.Equal(300, third);
        Assert.Equal(400, fourth);
        Assert.Equal(500, five);
        Assert.Equal(600, six);
        Assert.Equal(1000, sixAfter);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, -1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, -1)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 6)]
    public void RemoveAt_InvalidIndex_ThrowsIndexOutOfRangeException(int[] items, int index)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => smallList.RemoveAt(index));
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3, 4 }, 0, new int[] { 2, 3, 4 })]
    [InlineData(new int[] { 1, 2, 3, 4 }, 1, new int[] { 1, 3, 4 })]
    [InlineData(new int[] { 1, 2, 3, 4 }, 2, new int[] { 1, 2, 4 })]
    [InlineData(new int[] { 1, 2, 3, 4 }, 3, new int[] { 1, 2, 3 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 4, new int[] { 1, 2, 3, 4, 6 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 5, new int[] { 1, 2, 3, 4, 5 })]
    public void RemoveAt_MatchesExpectedItems(int[] items, int index, int[] expectedItems)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act
        smallList.RemoveAt(index);

        // Assert
        AssertList(smallList, expectedItems);
    }

    [Theory]
    [InlineData(new int[] { }, 1, -1)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 1, 0)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 2, 1)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 3, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 4, 3)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100, -1)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 5, 4)]
    public void IndexOf_MatchesExpectedContains(int[] items, int number, int expectedIndex)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act
        var actualIndex = smallList.IndexOf(number);

        // Assert
        Assert.Equal(items.Length, smallList.Count);
        Assert.Equal(expectedIndex, actualIndex);
    }

    [Theory]
    [InlineData(new int[] { }, 1, false)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 1, true)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 2, true)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 3, true)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 4, true)]
    [InlineData(new int[] { 1, 2, 3, 4 }, -1, false)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100, false)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 5, true)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 6, true)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 7, false)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, -1, false)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 8, true)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 9, true)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 11, false)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, -1, false)]
    public void Contains_MatchesExpectedContains(int[] items, int number, bool expectedContains)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act
        var containsNumber = smallList.Contains(number);

        // Assert
        Assert.Equal(items.Length, smallList.Count);
        Assert.Equal(expectedContains, containsNumber);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, -1)]
    [InlineData(new int[] { 1, 2 }, 2)]
    [InlineData(new int[] { 1, 2, 3, 4 }, 100)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, -1)]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 6)]
    public void Insert_InvalidIndex_ThrowsIndexOutOfRangeException(int[] items, int index)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => smallList.Insert(index, 42));
    }

    [Theory]
    [InlineData(new int[] { 1 }, 0, 123, new int[] { 123, 1 })]
    [InlineData(new int[] { 1, 2 }, 1, 123, new int[] { 1, 123, 2 })]
    [InlineData(new int[] { 1, 2, 3 }, 2, 123, new int[] { 1, 2, 123, 3 })]
    [InlineData(new int[] { 1, 2, 3, 4 }, 3, 123, new int[] { 1, 2, 3, 123, 4 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 4, 123, new int[] { 1, 2, 3, 4, 123, 5, 6 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 }, 5, 123, new int[] { 1, 2, 3, 4, 5, 123, 6 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 7, 123, new int[] { 1, 2, 3, 4, 5, 6, 7, 123, 8 })]
    public void Insert_MatchesExpectedItems(int[] items, int index, int item, int[] expectedItems)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act
        smallList.Insert(index, item);

        // Assert
        Assert.Equal(smallList.Count, expectedItems.Length);
        for (int i = 0; i < expectedItems.Length; i++)
        {
            Assert.Equal(expectedItems[i], smallList[i]);
        }
    }

    [Theory]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2 })]
    [InlineData(new int[] { 1, 2, 3 })]
    [InlineData(new int[] { 1, 2, 3, 4 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6 })]
    [InlineData(new int[] { 1, 2, 3, 4, 5, 6, 7 })]
    public void Clear_GivenItems_AllItemsAreRemoved(int[] items)
    {
        // Arrange
        var smallList = new SmallList<int>(items);

        // Act
        smallList.Clear();

        // Assert
        Assert.Empty(smallList);
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size. - I also want to assert the internal count
        Assert.Equal(0, smallList.Count);
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
        foreach (var item in items)
        {
            Assert.DoesNotContain(item, smallList);
        }
    }

    private static void AssertList(in SmallList<int> smallList, int[] expectedItems)
    {
        Assert.Equal(expectedItems.Length, smallList.Count);

        for (int i = 0; i < expectedItems.Length; i++)
        {
            Assert.Equal(expectedItems[i], smallList[i]);
        }
    }
}
