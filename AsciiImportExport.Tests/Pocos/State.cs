using System;

namespace AsciiImportExport.Tests.Pocos
{
    internal class State
    {
        public string ASSOCIATED { get; set; }
        public string CAR_PLATE { get; set; }
        public string CONTINENT { get; set; }
        public int? DATE_OF_FORMATION { get; set; }
        public int? DATE_OF_RESOLUTION { get; set; }
        public string DOCUMENT { get; set; }
        public MemberOfEU EU_MEMBER { get; set; }
        public string EXIST_ADD { get; set; }
        public string ISO_2 { get; set; }
        public string ISO_3 { get; set; }
        public int ISO_NUM { get; set; }
        public string NAME_ENG { get; set; }
        public string NAME_ENG_OFF { get; set; }
        public string NAME_GER { get; set; }
        public string NAME_GER_OFF { get; set; }
        public bool PART_OF_EU { get; set; }
        public string PREFIX { get; set; }
        public int STATUS { get; set; }
        public string TLD { get; set; }

        internal class MemberOfEU
        {
            public bool IsMember { get; set; }
            public int? Year { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (MemberOfEU)) return false;
                return Equals((MemberOfEU) obj);
            }

            protected bool Equals(MemberOfEU other)
            {
                return IsMember.Equals(other.IsMember) && Year == other.Year;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (IsMember.GetHashCode()*397) ^ Year.GetHashCode();
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (State)) return false;
            return Equals((State) obj);
        }

        protected bool Equals(State other)
        {
            return string.Equals(ASSOCIATED, other.ASSOCIATED) && string.Equals(CAR_PLATE, other.CAR_PLATE) && string.Equals(CONTINENT, other.CONTINENT) && DATE_OF_FORMATION == other.DATE_OF_FORMATION && DATE_OF_RESOLUTION == other.DATE_OF_RESOLUTION && string.Equals(DOCUMENT, other.DOCUMENT) && Equals(EU_MEMBER, other.EU_MEMBER) && string.Equals(EXIST_ADD, other.EXIST_ADD) && string.Equals(ISO_2, other.ISO_2) && string.Equals(ISO_3, other.ISO_3) && ISO_NUM == other.ISO_NUM && string.Equals(NAME_ENG, other.NAME_ENG) && string.Equals(NAME_ENG_OFF, other.NAME_ENG_OFF) && string.Equals(NAME_GER, other.NAME_GER) && string.Equals(NAME_GER_OFF, other.NAME_GER_OFF) && PART_OF_EU.Equals(other.PART_OF_EU) && string.Equals(PREFIX, other.PREFIX) && STATUS == other.STATUS && string.Equals(TLD, other.TLD);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (ASSOCIATED != null ? ASSOCIATED.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (CAR_PLATE != null ? CAR_PLATE.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (CONTINENT != null ? CONTINENT.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ DATE_OF_FORMATION.GetHashCode();
                hashCode = (hashCode*397) ^ DATE_OF_RESOLUTION.GetHashCode();
                hashCode = (hashCode*397) ^ (DOCUMENT != null ? DOCUMENT.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (EU_MEMBER != null ? EU_MEMBER.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (EXIST_ADD != null ? EXIST_ADD.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ISO_2 != null ? ISO_2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ISO_3 != null ? ISO_3.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ ISO_NUM;
                hashCode = (hashCode*397) ^ (NAME_ENG != null ? NAME_ENG.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (NAME_ENG_OFF != null ? NAME_ENG_OFF.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (NAME_GER != null ? NAME_GER.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (NAME_GER_OFF != null ? NAME_GER_OFF.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ PART_OF_EU.GetHashCode();
                hashCode = (hashCode*397) ^ (PREFIX != null ? PREFIX.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ STATUS;
                hashCode = (hashCode*397) ^ (TLD != null ? TLD.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}