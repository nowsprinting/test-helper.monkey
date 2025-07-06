// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.Paginators;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.Paginators
{
    [TestFixture]
    public class UguiScrollbarPaginatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";

        private GameObject _horizontalScrollbar;
        private GameObject _verticalScrollbar;

        [SetUp]
        public void SetUp()
        {
            var horizontalScrollView = GameObject.Find("Horizontal Scroll View");
            if (horizontalScrollView != null)
            {
                _horizontalScrollbar = horizontalScrollView.transform.Find("Scrollbar Horizontal")?.gameObject;
                if (_horizontalScrollbar != null)
                {
                    var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
                    scrollbar.value = 0f;
                }
            }

            var verticalScrollView = GameObject.Find("Vertical Scroll View");
            if (verticalScrollView != null)
            {
                _verticalScrollbar = verticalScrollView.transform.Find("Scrollbar Vertical")?.gameObject;
                if (_verticalScrollbar != null)
                {
                    var scrollbar = _verticalScrollbar.GetComponent<Scrollbar>();
                    scrollbar.value = 0f;
                }
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public void Constructor_ValidScrollbar_ObjectCreatedSuccessfully()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            
            var sut = new UguiScrollbarPaginator(scrollbar);
            
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_NullScrollbar_ThrowsArgumentNullException()
        {
            Assert.That(() => new UguiScrollbarPaginator(null), Throws.ArgumentNullException);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_ValidScrollbar_ValueBecomesZero()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 0.5f;
            var sut = new UguiScrollbarPaginator(scrollbar);

            await sut.ResetAsync();

            Assert.That(scrollbar.value, Is.EqualTo(0f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_NotAtEnd_ValueIncreasesAndReturnsTrue()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 0f;
            var sut = new UguiScrollbarPaginator(scrollbar);
            var beforeValue = scrollbar.value;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollbar.value, Is.GreaterThan(beforeValue));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_AtEnd_ValueNotChangedAndReturnsFalse()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 1f;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollbar.value, Is.EqualTo(1f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_FloatingPointPrecisionAtEnd_ReturnsFalse()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 1.0f - float.Epsilon;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_PageSizeBasedOnScrollbarSize_ValueIncreasesCorrectly()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 0f;
            scrollbar.size = 0.3f; // 30% of total content
            var sut = new UguiScrollbarPaginator(scrollbar);

            await sut.NextPageAsync();

            // Value increase should be based on scrollbar.size
            Assert.That(scrollbar.value, Is.EqualTo(scrollbar.size).Within(0.01f));
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_AtStart_ReturnsTrue()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 0f;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_AtMiddle_ReturnsTrue()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 0.5f;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_AtEnd_ReturnsFalse()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 1f;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_FloatingPointPrecisionAtEnd_ReturnsFalse()
        {
            var scrollbar = _horizontalScrollbar.GetComponent<Scrollbar>();
            scrollbar.value = 1.0f - float.Epsilon;
            var sut = new UguiScrollbarPaginator(scrollbar);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }
    }
}
