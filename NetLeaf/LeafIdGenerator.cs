using NetLeaf.Repositorys;

namespace NetLeaf
{
    internal class LeafIdGenerator : IIdGenerator
    {
        private readonly ILeafSegmentRepository _leafSegmentRepository;

        private bool _init;

        private long _previouId;

        private readonly SegmentBuffer _segmentBuffer;

        internal LeafIdGenerator(ILeafSegmentRepository leafSegmentRepository)
        {
            _leafSegmentRepository = leafSegmentRepository;
            _segmentBuffer = new SegmentBuffer();
        }

        void IIdGenerator.Init()
        {
            UpdateMaxIdAndGet(_segmentBuffer.Current);

            _init = true;
        }

        long IIdGenerator.Generation()
        {
            if (!_init)
            {
                throw new SystemException("LeafIdGenerator 未初始化成功。");
            }

            while (true)
            {
                _segmentBuffer.Lock.EnterReadLock();

                try
                {
                    var segment = _segmentBuffer.Current;

                    // 检查下号段是否只剩下 10%。
                    if (!_segmentBuffer.IsNextReady && segment.Surplus < 0.1 * segment.Step && _segmentBuffer.AtomicBoolean.CompareAndSet(false, true))
                    {
                        _segmentBuffer.Task = Task.Run(OpenSegmentBuffer);
                    }

                    var result = Interlocked.Increment(ref segment._value);

                    // 如果生成的号段未超出就直接返回。
                    if (result < segment.MaxId)
                    {
                        return result;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    // 释放读锁。
                    _segmentBuffer.Lock.ExitReadLock();
                }

                _segmentBuffer.Task?.Wait();

                _segmentBuffer.Lock.EnterWriteLock();

                try
                {
                    var segment = _segmentBuffer.Current;

                    var result = Interlocked.Increment(ref segment._value);

                    // 如果生成的号段未超出就直接返回。
                    if (result < segment.MaxId)
                    {
                        return result;
                    }

                    if (_segmentBuffer.IsNextReady)
                    {
                        // 切换位置。
                        _segmentBuffer.SwitchPos();
                        _segmentBuffer.IsNextReady = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _segmentBuffer.Lock.ExitWriteLock();
                }
            }
        }

        long IIdGenerator.Generation(out long previouId)
        {
            if (!_init)
            {
                throw new SystemException("LeafIdGenerator 未初始化成功。");
            }

            while (true)
            {
                _segmentBuffer.Lock.EnterReadLock();

                try
                {
                    var segment = _segmentBuffer.Current;

                    // 检查下号段是否只剩下 10%。
                    if (!_segmentBuffer.IsNextReady && segment.Surplus < 0.1 * segment.Step && _segmentBuffer.AtomicBoolean.CompareAndSet(false, true))
                    {
                        _segmentBuffer.Task = Task.Run(OpenSegmentBuffer);
                    }

                    var result = Interlocked.Increment(ref segment._value);

                    // 如果生成的号段未超出就直接返回。
                    if (result < segment.MaxId)
                    {
                        previouId = Interlocked.Exchange(ref _previouId, result);

                        return result;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    // 释放读锁。
                    _segmentBuffer.Lock.ExitReadLock();
                }

                _segmentBuffer.Task?.Wait();

                _segmentBuffer.Lock.EnterWriteLock();

                try
                {
                    var segment = _segmentBuffer.Current;

                    var result = Interlocked.Increment(ref segment._value);

                    // 如果生成的号段未超出就直接返回。
                    if (result < segment.MaxId)
                    {
                        previouId = Interlocked.Exchange(ref _previouId, result);

                        return result;
                    }

                    if (_segmentBuffer.IsNextReady)
                    {
                        // 切换位置。
                        _segmentBuffer.SwitchPos();
                        _segmentBuffer.IsNextReady = false;
                        // 切换号段完成。
                    }
                    else
                    {
                        continue;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _segmentBuffer.Lock.ExitWriteLock();
                }
            }
        }

        void UpdateMaxIdAndGet(Segment segment)
        {
            var leafSegment = _leafSegmentRepository.UpdateMaxIdAndGet();

            segment.MaxId = leafSegment.MaxId;
            segment.Step = leafSegment.Step;
            segment.Value = leafSegment.MaxId - leafSegment.Step;
        }

        void OpenSegmentBuffer()
        {
            bool isOK = false;

            try
            {
                var segment = _segmentBuffer[_segmentBuffer.NextPos()];

                // 去数据库中更新号段。
                UpdateMaxIdAndGet(segment);

                isOK = true;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (isOK)
                {
                    // 进入写入锁。
                    _segmentBuffer.Lock.EnterWriteLock();

                    _segmentBuffer.IsNextReady = true;
                    _segmentBuffer.AtomicBoolean.Set(false);

                    _segmentBuffer.Lock.ExitWriteLock();
                }
                else
                {
                    _segmentBuffer.AtomicBoolean.Set(false);
                }
            }
        }

        internal class Segment
        {
            internal long _value;

            internal long MaxId { get; set; }

            internal int Step { get; set; }

            /// <summary>
            /// 更新时间。
            /// </summary>
            internal long Time { get; set; }

            /// <summary>
            /// 当前累计的值。
            /// </summary>
            internal long Value
            {
                get { return _value; }
                set { _value = value; }
            }

            /// <summary>
            /// 获取剩余号段。
            /// </summary>
            internal long Surplus
            {
                get
                {
                    return MaxId - _value;
                }
            }

        }

        internal class SegmentBuffer
        {
            private int _currentPos;
            private Segment[] _segments;
            private ReaderWriterLockSlim _lock;
            private AtomicBoolean _atomicBoolean;

            /// <summary>
            /// 是否正在更新中。
            /// </summary>
            internal bool IsUpdating { get; set; }

            /// <summary>
            /// 下一个号段是否可以读取。
            /// </summary>
            internal bool IsNextReady { get; set; }

            internal Task Task { get; set; }

            /// <summary>
            /// 获取读写锁。
            /// </summary>
            internal ReaderWriterLockSlim Lock => _lock;

            internal Segment Current => _segments[_currentPos];

            internal AtomicBoolean AtomicBoolean => _atomicBoolean;

            internal Segment this[int pos]
            {
                get { return _segments[pos]; }
            }

            internal SegmentBuffer()
            {
                _segments = new Segment[] { new Segment(), new Segment() };
                _lock = new ReaderWriterLockSlim();
                _atomicBoolean = new AtomicBoolean(false);
            }

            internal int NextPos()
            {
                return (_currentPos + 1) % 2;
            }

            internal void SwitchPos()
            {
                _currentPos = NextPos();
            }
        }
    }
}
