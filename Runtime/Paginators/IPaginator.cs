// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;

namespace TestHelper.Monkey.Paginators
{
    /// <summary>
    /// ページング可能なUIコンポーネントを制御するためのインターフェース。
    /// ページ送り機能として直感的なページネーション操作を提供し、
    /// UI要素探索時の補助操作を実現する。
    /// </summary>
    public interface IPaginator
    {
        /// <summary>
        /// ページ位置を先頭に移動する。
        /// スクロールコンポーネントの場合、表示上の位置（上端か下端か左端か右端か）は実装依存。
        /// </summary>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        UniTask ResetAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 次のページに移動する。
        /// スクロールコンポーネントの場合、表示領域サイズ分だけページを進める。
        /// </summary>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>ページ送りが実行された場合true、終端で実行されなかった場合false</returns>
        UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 次のページが存在するかどうかを取得する
        /// </summary>
        /// <returns>次のページが存在する場合true、終端に達している場合false</returns>
        bool HasNextPage();
    }
}