// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace TestHelper.Monkey.Paginators
{
    /// <summary>
    /// Interface for controlling pageable UI components.
    /// Provides intuitive pagination operations as page navigation functionality, enabling auxiliary operations during UI element exploration.
    /// </summary>
    public interface IPaginator
    {
        /// <summary>
        /// Move the page position to the beginning.
        /// For scroll components, the display position (top, bottom, left, or right) depends on the implementation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        UniTask ResetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Move to the next page.
        /// For scroll components, advance the page by the size of the display area.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if page navigation was executed, false if not executed at the end</returns>
        UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get whether the next page exists
        /// </summary>
        /// <returns>True if the next page exists, false if the end has been reached</returns>
        bool HasNextPage();
    }
}
