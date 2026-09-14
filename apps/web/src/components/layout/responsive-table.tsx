import type { ReactNode } from "react";
import { Fragment } from "react";
import { cn } from "@/lib/utils";
import { DataFieldList, type DataField } from "@/components/layout/data-field-list";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

export type ResponsiveTableColumn<T> = {
  header: ReactNode;
  headerClassName?: string;
  cellClassName?: string;
  render: (row: T) => ReactNode;
  mobileLabel?: string;
  mobileHidden?: boolean;
  mobileProminent?: boolean;
};

type Props<T> = {
  rows: T[];
  rowKey: (row: T) => string;
  columns?: ResponsiveTableColumn<T>[];
  renderDesktop?: () => ReactNode;
  renderMobileCard?: (row: T) => ReactNode;
  mobileCardClassName?: string;
  tableClassName?: string;
  desktopClassName?: string;
  emptyMessage?: ReactNode;
};

function defaultMobileCard<T>(
  row: T,
  columns: ResponsiveTableColumn<T>[],
  className: string | undefined,
  key: string,
) {
  const prominent = columns.filter((c) => c.mobileProminent);
  const fields: DataField[] = columns
    .filter((c) => c.mobileLabel && !c.mobileHidden && !c.mobileProminent)
    .map((c) => ({
      label: c.mobileLabel!,
      value: c.render(row),
    }));

  return (
    <li key={key} className={cn("rounded border border-border bg-card p-4 text-sm", className)}>
      {prominent.length > 0 ? (
        <div className="space-y-2">
          {prominent.map((col, i) => (
            <div key={i}>{col.render(row)}</div>
          ))}
        </div>
      ) : null}
      {fields.length > 0 ? (
        <DataFieldList fields={fields} className={prominent.length > 0 ? "mt-2" : undefined} />
      ) : null}
    </li>
  );
}

export function ResponsiveTable<T>({
  rows,
  rowKey,
  columns = [],
  renderDesktop,
  renderMobileCard,
  mobileCardClassName,
  tableClassName,
  desktopClassName,
  emptyMessage,
}: Props<T>) {
  if (rows.length === 0 && emptyMessage) {
    return <>{emptyMessage}</>;
  }

  return (
    <>
      <div className={cn("hidden md:block", desktopClassName)}>
        {renderDesktop ? (
          renderDesktop()
        ) : (
          <Table className={tableClassName}>
            <TableHeader>
              <TableRow>
                {columns.map((col, i) => (
                  <TableHead key={i} className={col.headerClassName}>
                    {col.header}
                  </TableHead>
                ))}
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((row) => (
                <TableRow key={rowKey(row)}>
                  {columns.map((col, i) => (
                    <TableCell key={i} className={col.cellClassName}>
                      {col.render(row)}
                    </TableCell>
                  ))}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>

      <ul className="space-y-3 md:hidden">
        {rows.map((row) =>
          renderMobileCard ? (
            <Fragment key={rowKey(row)}>{renderMobileCard(row)}</Fragment>
          ) : (
            defaultMobileCard(row, columns, mobileCardClassName, rowKey(row))
          ),
        )}
      </ul>
    </>
  );
}
