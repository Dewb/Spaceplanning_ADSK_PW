//
//  RuleToken.cpp
//  burningbush
//
//  Created by Michael Dewberry on 1/30/15.
//
//

#include "LSystem.h"
#include <iostream>


RuleToken::RuleToken(const string& str, int* pStartPosition) {
    int pos = 0;
    if (pStartPosition) {
        pos = *pStartPosition;
    }
    symbol.push_back(str[pos]);
    pos++;
    if (pos + 1 < str.size() && str[pos] == '_') {
        subscript = str[pos + 1];
        pos += 2;
    }
    if (pos + 1 < str.size() && str[pos] == '(') {
        int parenLevel = 1;
        pos = pos + 1;
        int startPos = pos;
        while (parenLevel > 0 && pos < str.size()) {
            if (str[pos] == ')') {
                parenLevel--;
                pos++;
            } else if (str[pos] == '(') {
                parenLevel++;
                pos++;
            } else if (str[pos] == ',' && parenLevel == 1) {
                parameters.push_back(str.substr(startPos, pos - startPos));
                pos++;
                startPos = pos;
            } else {
                pos++;
            }
        }
        if (parenLevel != 0) {
            cout << "ERROR: Unmatched parentheses in token!\n";
        } else if (pos < str.size() + 1) {
            parameters.push_back(str.substr(startPos, pos - startPos - 1));
        }
    }
    if (pStartPosition) {
        *pStartPosition = pos;
    }
}

RuleToken::RuleToken(const char* cstr) {
    string str(cstr);
    *this = RuleToken(str);
}

RuleToken::RuleToken(const char c) {
    symbol.push_back(c);
}

bool RuleToken::operator<(const RuleToken& rhs) const {
    if (this->symbol == rhs.symbol) {
        if (this->subscript == rhs.subscript) {
            return this->parameters.size() < rhs.parameters.size();
        } else {
            return this->subscript < rhs.subscript;
        }
    } else {
        return this->symbol < rhs.symbol;
    }
}

bool RuleToken::operator>(const RuleToken& rhs) const {
    if (this->symbol == rhs.symbol) {
        if (this->subscript == rhs.subscript) {
            return this->parameters.size() > rhs.parameters.size();
        } else {
            return this->subscript > rhs.subscript;
        }
    } else {
        return this->symbol > rhs.symbol;
    }
}

bool RuleToken::operator==(const RuleToken& rhs) const {
    if (this->symbol == rhs.symbol) {
        if (this->subscript == rhs.subscript) {
            return this->parameters.size() == rhs.parameters.size();
        } else {
            return this->subscript == rhs.subscript;
        }
    } else {
        return false;
    }
}

bool RuleToken::operator!=(const RuleToken& rhs) const {
    if (this->symbol == rhs.symbol) {
        if (this->subscript == rhs.subscript) {
            return this->parameters.size() != rhs.parameters.size();
        } else {
            return this->subscript != rhs.subscript;
        }
    } else {
        return true;
    }
}

bool RuleToken::operator==(const string& rhs) const {
    return this->symbol == rhs;
}

bool RuleToken::operator==(const char& rhs) const {
    return this->symbol.size() == 1 && this->symbol[0] == rhs;
}

std::ostream& operator<<(std::ostream& os, const RuleToken& token) {
    os << token.symbol;
    if (!token.subscript.empty()) {
        os << "_" << token.subscript;
    }
    if (token.parameters.size() > 0) {
        os << "(";
        for (int i = 0; i < token.parameters.size(); i++) {
            os << token.parameters[i];
            if (i != token.parameters.size() - 1) {
                os << ",";
            }
        }
        os << ")";
    }
    return os;
}

string to_string(const RuleToken& rt) {
    ostringstream ss;
    ss << rt;
    return ss.str();
}
