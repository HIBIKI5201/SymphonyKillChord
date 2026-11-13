#include "OutputText.h"

namespace skc::launcher
{
    namespace
    {
        constexpr char ESCAPE = '\x1b';
        constexpr char BELL = '\x07';

        /// <summary>
        ///     UTF-8 の先頭バイトから、続くバイト数を返す。先頭として正しくなければ -1。
        /// </summary>
        int ContinuationCount(unsigned char lead)
        {
            if (lead < 0x80) { return 0; }
            if (lead >= 0xC2 && lead <= 0xDF) { return 1; }
            if (lead >= 0xE0 && lead <= 0xEF) { return 2; }
            if (lead >= 0xF0 && lead <= 0xF4) { return 3; }

            return -1;
        }
    }

    std::vector<std::string> OutputLineBuffer::Feed(const std::string& chunk)
    {
        std::vector<std::string> lines;
        _pending += chunk;

        std::size_t start = 0;
        std::size_t newline;
        while ((newline = _pending.find('\n', start)) != std::string::npos)
        {
            std::size_t end = newline;
            if (end > start && _pending[end - 1] == '\r') { --end; }
            lines.push_back(_pending.substr(start, end - start));
            start = newline + 1;
        }

        _pending.erase(0, start);
        return lines;
    }

    std::string OutputLineBuffer::Flush()
    {
        std::string rest;
        rest.swap(_pending);
        if (!rest.empty() && rest.back() == '\r') { rest.pop_back(); }

        return rest;
    }

    bool IsValidUtf8(const std::string& text)
    {
        std::size_t i = 0;
        while (i < text.size())
        {
            unsigned char lead = static_cast<unsigned char>(text[i]);
            int count = ContinuationCount(lead);
            if (count < 0 || i + static_cast<std::size_t>(count) >= text.size()) { return false; }

            for (int k = 1; k <= count; ++k)
            {
                unsigned char next = static_cast<unsigned char>(text[i + k]);
                if ((next & 0xC0) != 0x80) { return false; }
            }

            // 冗長な表し方・サロゲート・範囲外を弾く。
            if (count == 2)
            {
                unsigned char second = static_cast<unsigned char>(text[i + 1]);
                if (lead == 0xE0 && second < 0xA0) { return false; }
                if (lead == 0xED && second > 0x9F) { return false; }
            }
            else if (count == 3)
            {
                unsigned char second = static_cast<unsigned char>(text[i + 1]);
                if (lead == 0xF0 && second < 0x90) { return false; }
                if (lead == 0xF4 && second > 0x8F) { return false; }
            }

            i += static_cast<std::size_t>(count) + 1;
        }

        return true;
    }

    std::string StripAnsi(const std::string& line)
    {
        std::string result;
        result.reserve(line.size());

        std::size_t i = 0;
        while (i < line.size())
        {
            if (line[i] != ESCAPE)
            {
                result.push_back(line[i]);
                ++i;
                continue;
            }

            if (i + 1 >= line.size()) { break; }

            char kind = line[i + 1];
            i += 2;
            if (kind == '[')
            {
                // CSI: 引数のあと、0x40〜0x7E の1文字で終わる。
                while (i < line.size() && !(line[i] >= 0x40 && line[i] <= 0x7E)) { ++i; }
                if (i < line.size()) { ++i; }
            }
            else if (kind == ']')
            {
                // OSC: BEL か ESC \ で終わる。
                while (i < line.size())
                {
                    if (line[i] == BELL) { ++i; break; }
                    if (line[i] == ESCAPE && i + 1 < line.size() && line[i + 1] == '\\') { i += 2; break; }
                    ++i;
                }
            }

            // それ以外の2文字のシーケンスは、ESC と次の1文字を捨てるだけでよい。
        }

        return result;
    }
}
