import React, { useEffect, useState } from 'react';
import { Grid, GridColumn, GridToolbar } from '@progress/kendo-react-grid';
import { DropDownList } from '@progress/kendo-react-dropdowns';
import { Rating } from '@progress/kendo-react-inputs';
import { Sparkline } from '@progress/kendo-react-charts';
import '@progress/kendo-theme-default/dist/all.css'; // Import Kendo UI styles
const categories = [
  { CategoryID: 1, CategoryName: 'Beverages' },
  { CategoryID: 2, CategoryName: 'Condiments' },
  { CategoryID: 3, CategoryName: 'Confections' },
  { CategoryID: 4, CategoryName: 'Dairy Products' },
  { CategoryID: 5, CategoryName: 'Grains/Cereals' },
  { CategoryID: 6, CategoryName: 'Meat/Poultry' },
  { CategoryID: 7, CategoryName: 'Produce' },
  { CategoryID: 8, CategoryName: 'Seafood' },
];

const crudServiceBaseUrl = 'https://demos.telerik.com/kendo-ui/service';

interface Product {
  ProductName: string;
  UnitPrice: number;
  Discontinued: boolean;
  Category: { CategoryName: string };
  CustomerRating: number;
  Country: { CountryNameLong: string };
  UnitsInStock: number;
  TotalSales: number;
  TargetSales: number;
}

const KendoPractice: React.FC = () => {
  const [data, setData] = useState<Product[]>([]);

  useEffect(() => {
    // Fetch the data for the grid
    fetch(`${crudServiceBaseUrl}/detailproducts?callback=?`)
      .then((response) => response.json())
      .then((result) => setData(result));
  }, []);

  const clientCategoryEditor = (props: any) => (
    <DropDownList {...props} data={categories} textField="CategoryName" dataItemKey="CategoryID" />
  );

  const clientCountryEditor = (props: any) => (
    <DropDownList {...props} data={['Bulgaria', 'United States', 'Canada']} />
  );

  const renderSparkline = (data: number[]) => (
    <Sparkline
      data={data}
      type="bar"
    //   chartArea={{
    //     margin: 0,
    //     width: 180,
    //     background: 'transparent',
    //   }}
      seriesDefaults={{
        labels: { visible: true, format: '{0}%', background: 'none' },
      }}
    //   categoryAxis={{
    //     majorGridLines: { visible: false },
    //     majorTicks: { visible: false },
    //   }}
    //   valueAxis={{ type: 'numeric', min: 0, max: 130, visible: false }}
    //   tooltip={{ visible: false }}
    />
  );

  return (
    <div>
      <Grid
        data={data}
        pageable
        sortable
        resizable
        groupable
        filterable
        // editable="incell"
        // height={680}
        pageSize={20}
        // batch={true}
        // autoSync={true}
        // aggregate={[{ field: 'TotalSales', aggregate: 'sum' }]}
        // group={{
        //   field: 'Category.CategoryName',
        //   dir: 'desc',
        //   aggregates: [{ field: 'TotalSales', aggregate: 'sum' }],
        // }}
      >
        <GridToolbar>
          <button
            title="Export to Excel"
            onClick={() => {
              // Replace with actual logic for exporting
              alert('Export to Excel');
            }}
          >
            Export to Excel
          </button>
          <button
            title="Export to PDF"
            onClick={() => {
              // Replace with actual logic for exporting
              alert('Export to PDF');
            }}
          >
            Export to PDF
          </button>
        </GridToolbar>

        <GridColumn field="ProductName" title="Product Name" width="300" />
        <GridColumn field="UnitPrice" title="Price" format="{0:c}" width="105" />
        <GridColumn field="Discontinued" title="In Stock" width="130" />
        <GridColumn
          field="Category.CategoryName"
          title="Category"
        //   editor={clientCategoryEditor}
          width="125"
        />
        <GridColumn
          field="CustomerRating"
          title="Rating"
          width="200"
          cell={(props) => (
            <td>
              <Rating
                value={props.dataItem.CustomerRating}
                min={1}
                max={5}
                readonly={true}
              />
            </td>
          )}
        />
        <GridColumn
          field="TargetSales"
          title="Target Sales"
          width="220"
          cell={(props) => (
            <td>
              {renderSparkline([props.dataItem.TargetSales])}
            </td>
          )}
        />
        <GridColumn field="UnitsInStock" title="Units" width="105" />
        <GridColumn field="TotalSales" title="Total Sales" format="{0:c}" width="140" 
        // aggregates={['sum']} 
        />
        <GridColumn
          field="Country.CountryNameLong"
          title="Country"
        //   editor={clientCountryEditor}
          width="120"
        />
        <GridColumn
        //   command="destroy"
          title="Delete"
          width="120"
        />
      </Grid>
    </div>
  );
};

export default KendoPractice;
