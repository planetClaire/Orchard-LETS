using System;
using System.Linq;
using LETS.Models;
using Orchard.Data;

namespace LETS.Services
{
    public class StatsService : IStatsService
    {
        private readonly IMemberService _memberService;
        private readonly IRepository<DailyStatsRecord> _dailyStatsRepository;
        private readonly INoticeService _noticeService;
        private readonly IRepository<NoticeStatsRecord> _noticeStatsRepository;

        public StatsService(IMemberService memberService, IRepository<DailyStatsRecord> dailyStatsRepository, INoticeService noticeService, IRepository<NoticeStatsRecord> noticeStatsRepository) {
            _memberService = memberService;
            _dailyStatsRepository = dailyStatsRepository;
            _noticeService = noticeService;
            _noticeStatsRepository = noticeStatsRepository;
        }

        public void SaveStats() {
            var dailyStatsRecord = new DailyStatsRecord {
                DateCollected = DateTime.UtcNow,
                TotalTurnover = _memberService.GetTotalTurnover(),
                MemberCount = _memberService.GetMemberParts(MemberType.Member).Count()
            };
            _dailyStatsRepository.Create(dailyStatsRecord);
            var noticeTypes = _noticeService.GetNoticeTypes();
            foreach (var noticeStatsRecord in noticeTypes.Select(noticeType => new NoticeStatsRecord
            {
                DateCollected = DateTime.UtcNow,
                IdNoticeType = noticeType.Id,
                NoticeCount = _noticeService.GetNoticeCountByType(noticeType.Id)
            }))
            {
                _noticeStatsRepository.Create(noticeStatsRecord);
            }
        }
    }
}