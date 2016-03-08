//
//  RuleString.cpp
//  burningbush
//
//  Created by Michael Dewberry on 1/30/15.
//
//

#include "LSystem.h"

RuleString::RuleString(const string& str) {
    RuleString r;
    int i = 0;
    while(i < str.size()) {
        RuleToken token(str, &i);
        push_back(token);
    }
}

RuleString::RuleString(const char* c) {
    string str(c);
    RuleString r;
    int i = 0;
    while(i < str.size()) {
        RuleToken token(str, &i);
        push_back(token);
    }
}

std::ostream& operator<<(std::ostream& os, const RuleString& str) {
    for (auto& token : str) {
        os << token;
    }
    return os;
}

string to_string(const RuleString& rs) {
    ostringstream ss;
    for (const RuleToken& t : rs) {
        ss << t;
    }
    return ss.str();
}