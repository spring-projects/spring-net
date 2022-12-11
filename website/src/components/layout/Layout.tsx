import * as React from "react";
import { Helmet } from "react-helmet";
import Footer from "./Footer";
import Header from "./Header";
import useSiteMetadata from "../utils/SiteMetadata";
import { ReactNode } from "react";
import "../../styles/main.scss";
import { Seo } from "../common/Seo";

interface Seo {
  title?: string;
  description?: string;
  keywords?: Array<string>;
  image?: string;
  siteUrl?: string;
  hasNoSpring?: boolean;
}
interface Props {
  children: ReactNode;
  className?: string;
  seo?: Seo;
}

const mergeMetadata = (generic: Seo, specific?: Seo): Seo => {
  const meta = { ...generic };
  if (!specific) {
    return meta;
  }
  if (specific?.title) {
    meta.title = specific.title;
  }
  if (specific?.description) {
    meta.description = specific?.description;
  }
  if (specific?.keywords && specific?.keywords?.length > 0) {
    meta.keywords = specific.keywords;
  }
  if (specific?.hasNoSpring) {
    meta.hasNoSpring = true;
  }
  return meta;
};

const Layout = ({ children, className, seo }: Props) => {
  const metadata = mergeMetadata(useSiteMetadata(), seo);
  return (
    <div>
      <Seo metadata={metadata} />
      <Header />
      <div className={`main ${className}`}>{children}</div>
      <Footer />
    </div>
  );
};

export default Layout;
