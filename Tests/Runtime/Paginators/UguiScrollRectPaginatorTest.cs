// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Paginators;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Tests.Paginators
{
    [TestFixture]
    public class UguiScrollRectPaginatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";

        private GameObject _horizontalScrollView;
        private GameObject _verticalScrollView;
        private GameObject _bothScrollView;
        private GameObject _disabledScrollView;

        [SetUp]
        public void SetUp()
        {
            _horizontalScrollView = GameObject.Find("Horizontal Scroll View");
            _verticalScrollView = GameObject.Find("Vertical Scroll View");
            _bothScrollView = GameObject.Find("Both Scroll View");
            _disabledScrollView = GameObject.Find("Disabled Scroll View");

            if (_horizontalScrollView != null)
            {
                var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
                scrollRect.normalizedPosition = new Vector2(0f, 0f);
            }

            if (_verticalScrollView != null)
            {
                var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }

            if (_bothScrollView != null)
            {
                var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public void Constructor_ValidScrollRect_ObjectCreatedSuccessfully()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            
            var sut = new UguiScrollRectPaginator(scrollRect);
            
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_NullScrollRect_ThrowsArgumentNullException()
        {
            Assert.That(() => new UguiScrollRectPaginator(null), Throws.ArgumentNullException);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_HorizontalScrollRect_NormalizedPositionBecomesZeroZero()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_VerticalScrollRect_NormalizedPositionBecomesZeroZero()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_BothScrollRect_NormalizedPositionBecomesZeroZero()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_HorizontalScrollNotAtEnd_ScrollsHorizontallyAndReturnsTrue()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.GreaterThan(beforePosition.x));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_HorizontalScrollAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(1f).Within(float.Epsilon));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_VerticalScrollNotAtEnd_ScrollsVerticallyAndReturnsTrue()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.y, Is.LessThan(beforePosition.y));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_VerticalScrollAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(0f).Within(float.Epsilon));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollHorizontalNotAtEnd_ScrollsHorizontallyAndReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.GreaterThan(beforePosition.x));
            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(beforePosition.y));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollHorizontalAtEnd_ResetsXAndScrollsVerticallyAndReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(0f).Within(float.Epsilon));
            Assert.That(scrollRect.normalizedPosition.y, Is.LessThan(1f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollBothAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(1f, 0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_ScrollDisabled_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalScrollAtStart_ReturnsTrue()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalScrollAtMiddle_ReturnsTrue()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalScrollAtEnd_ReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalScrollAtStart_ReturnsTrue()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalScrollAtMiddle_ReturnsTrue()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalScrollAtEnd_ReturnsFalse()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollAtStart_ReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollHorizontalAtEnd_ReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollVerticalAtEnd_ReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollBothAtEnd_ReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_ScrollDisabled_ReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_FloatingPointPrecisionAtEnd_ReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1.0f - float.Epsilon, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_FloatingPointPrecisionAtStart_ReturnsTrue()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0.0f + float.Epsilon);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }
    }
}